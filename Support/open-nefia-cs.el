;;; open-nefia-cs-.el --- Emacs integration with OpenNefia  -*- lexical-binding: t; -*-

;; Copyright (C) 2019-2022  Ruin0x11

;; Author: Ruin0x11 <ipickering2@gmail.com>
;; Keywords: processes, tools

;; This program is free software; you can redistribute it and/or modify
;; it under the terms of the GNU General Public License as published by
;; the Free Software Foundation, either version 3 of the License, or
;; (at your option) any later version.

;; This program is distributed in the hope that it will be useful,
;; but WITHOUT ANY WARRANTY; without even the implied warranty of
;; MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;; GNU General Public License for more details.

;; You should have received a copy of the GNU General Public License
;; along with this program.  If not, see <https://www.gnu.org/licenses/>.

;;; Commentary:

;; A library that lets you interface with OpenNefia's debug server.

;;; Code:

(provide 'open-nefia-cs)

(require 'projectile)
(require 'eval-sexp-fu nil t)
(require 'json)
(require 'dash)
(require 'company)
(require 'cl)
(require 'flycheck)
(require 'markdown-mode)
(require 'help-mode)
(require 'eros)

(defvar open-nefia-cs-minor-mode-map
  (let ((map (make-sparse-keymap)))
    (define-key map "\C-c\C-l" 'open-nefia-cs-send-buffer)
    map))

(defcustom open-nefia-cs-repl-address "127.0.0.1"
  "Address to use for connecting to the REPL.")

(defcustom open-nefia-cs-repl-port 4567
  "Port to use for connecting to the REPL.")

;;;###autoload
(define-minor-mode open-nefia-cs-minor-mode
  "Elona next debug server."
  :lighter " Open-Nefia-C#" :keymap open-nefia-cs-minor-mode-map)

(defun open-nefia-cs--parse-response ()
  (condition-case nil
      (progn
        (goto-char (point-min))
        (if (fboundp 'json-parse-buffer)
            (json-parse-buffer
             :object-type 'alist
             :null-object nil
             :false-object :json-false)
          (json-read)))
    (error nil)))

(defun open-nefia-cs--tcp-filter (proc chunk)
  (with-current-buffer (process-buffer proc)
    (goto-char (point-max))
    (insert chunk)
    (let ((response (process-get proc :response)))
      (unless response
        (when (setf response (open-nefia-cs--parse-response))
          (delete-region (point-min) (point))
          (process-put proc :response response)))))
  (when-let ((response (process-get proc :response)))
    (with-current-buffer (process-buffer proc)
      (erase-buffer))
    (process-put proc :response nil)
    (with-demoted-errors "Error: %s"
        (open-nefia-cs--process-response
         (process-get proc :command)
         (process-get proc :args)
         response))))

(defun open-nefia-cs--process-response (cmd args response)
  (with-demoted-errors "Error: %s"
    (-let (((&alist 'success 'message) response))
      (if (eq success t)
          (pcase cmd
            ("exec" (let ((result (alist-get 'result response)))
                      (message "--> %s" result)
                      (eros--make-result-overlay result
                        :where (line-end-position)
                        :duration 5)))
            (else (error "No action for %s %s" cmd (prin1-to-string response))))
        (error message)))))

;;
;; Network
;;

(defun open-nefia-cs--tcp-sentinel (proc message)
  "Runs when a client closes the connection."
  (when (string-match-p "^open " message)
    (let ((buffer (process-buffer proc)))
      (when buffer
        (kill-buffer buffer)))))

(defun open-nefia-cs--make-tcp-connection (host port)
  (make-network-process :name "Open-Nefia-C#"
                        :buffer "*Open-Nefia-C#*"
                        :host host
                        :service port
                        :filter 'open-nefia-cs--tcp-filter
                        :sentinel 'open-nefia-cs--tcp-sentinel
                        :coding 'utf-8))

(defun open-nefia-cs--send (cmd args)
  (let ((proc (open-nefia-cs--make-tcp-connection open-nefia-cs-repl-address open-nefia-cs-repl-port))
        (json (json-encode (list :command cmd :args args))))
    (when (process-live-p proc)
      (process-put proc :command cmd)
      (process-put proc :args args)
      (comint-send-string proc (format "%s\n" json))
      (process-send-eof proc))))

(defun open-nefia-cs--unbracket-string (pre post string)
  "Remove PRE/POST from the beginning/end of STRING.
Both PRE and POST must be pre-/suffixes of STRING, or neither is
removed.  Return the new string.  If STRING is nil, return nil."
  (declare (indent 2))
  (and string
       (if (and (string-prefix-p pre string)
		(string-suffix-p post string))
	   (substring string (length pre) (- (length post)))
	 string)))

(defsubst open-nefia-cs--escape-string (str)
  "Escape quotes and newlines in STR."
  (replace-regexp-in-string
   "\n" "\\\\n"
   (replace-regexp-in-string
    "\"" "\\\\\""
    (replace-regexp-in-string
     "'" "\\\\'" str))))

(defsubst open-nefia-cs--unescape-string (str)
  "Unescape escaped commas, semicolons and newlines in STR."
  (open-nefia-cs--unbracket-string "'" "'"
    (replace-regexp-in-string
     "\\\\n" "\n"
     (replace-regexp-in-string
      "\\\\\\([,;]\\)" "\\1" str))))

(defun open-nefia-cs--send-to-repl (str)
  (open-nefia-cs--send "exec" (list :script (format (open-nefia-cs--escape-string str)))))

(defun open-nefia-cs-send-region (start end)
  (interactive "r")
  (open-nefia-cs--send-to-repl (buffer-substring-no-properties start end)))

(defun open-nefia-cs-send-buffer ()
  (interactive)
  (open-nefia-cs-send-region (point-min) (point-max)))

(defun open-nefia-cs-send-current-line ()
  (interactive)
  (open-nefia-cs-send-region (line-beginning-position) (line-end-position)))

(defun open-nefia-cs--bounds-of-buffer ()
  (cons (point-min) (point-max)))

(defun open-nefia-cs--bounds-of-line ()
  (cons (point-at-bol) (point-at-eol)))

(defun open-nefia-cs--project-root ()
  (let ((root (projectile-project-root)))
    (projectile-locate-dominating-file root "OpenNefia.sln")))

(provide 'open-nefia-cs)
