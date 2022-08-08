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
    (open-nefia-cs--process-response
     (process-get proc :command)
     (process-get proc :args)
     response)))

(defun open-nefia-cs--process-response (cmd args response)
  (let ((process (open-nefia-cs-repl-process)))
    (condition-case err
        (-let (((&alist 'success 'message) response))
          (if (eq success t)
              (pcase cmd
                ("exec"
                 (let ((result (alist-get 'result response)))
                   (if (process-live-p process)
                       (progn
                         (comint-output-filter process result)
                         (comint-output-filter process "\n")
                         (comint-output-filter process open-nefia-cs-repl-prompt))
                     (message "--> %s" result)
                     (eros--make-result-overlay result
                       :where (line-end-position)
                       :duration 5))))
                (else (error "No action for %s %s" cmd (prin1-to-string response))))
            (error message)))
        (error
         (if (process-live-p process)
             (progn
               (comint-output-filter process (format "Error: %s\n" (error-message-string err)))
               (comint-output-filter process open-nefia-cs-repl-prompt))
           (message "Error: %s" err))))))

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
  (open-nefia-cs-repl-input-sender
   (open-nefia-cs-repl-process)
   (buffer-substring-no-properties start end)
   t))

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

(defun open-nefia-cs--executable-name ()
  (if (eq system-type 'windows-nt) "OpenNefia.EntryPoint.exe" "OpenNefia.EntryPoint"))

(defun open-nefia-cs--executable-path ()
  (concat (file-name-as-directory (open-nefia-cs--project-root))
          "OpenNefia.EntryPoint/bin/Debug/net6.0/"
          (open-nefia-cs--executable-name)))

(defun open-nefia-cs-run-headlessly (arg)
  (interactive "P")
  (let* ((script-file (buffer-file-name))
         (exe (open-nefia-cs--executable-path))
         (cmd (format "%s exec %s" exe script-file))
         (default-directory (file-name-directory exe)))
    (compile cmd)))

(defun open-nefia-cs-jump-to-other-locale-file ()
  (interactive)
  (let* ((file (buffer-file-name)))
    (when (string-match "\\(.*\\)/\\(en_US\\|ja_JP\\|en\\|jp\\)/\\(.*\\)" file)
      (let* ((prefix (match-string 1 file))
             (lang (pcase (match-string 2 file)
                     ("en" "jp")
                     ("jp" "en")
                     ("en_US" "ja_JP")
                     (else "en_US")))
             (suffix (match-string 3 file))
             (new-file (concat prefix "/" lang "/" suffix))
             (new-dir (file-name-directory new-file))
             (old-line (line-number-at-pos (point))))
        (when (not (file-directory-p new-dir))
          (make-directory new-dir))
        (find-file new-file)
        (goto-line old-line)))))

(defun open-nefia-cs--fontify-region (mode beg end)
  (let ((prev-mode major-mode))
    (delay-mode-hooks (funcall mode))
    (font-lock-default-function mode)
    (font-lock-default-fontify-region beg end nil)
    ;(delay-mode-hooks (funcall prev-mode))
    ))

(defun open-nefia-cs--fontify-str (str mode)
  (with-temp-buffer
    (insert str)
    (open-nefia-cs--fontify-region mode (point-min) (point-max))
    (buffer-string)))

;; keys for interacting inside Monroe REPL buffer
(defvar open-nefia-cs-repl-mode-map
  (let ((map (make-sparse-keymap)))
    (set-keymap-parent map comint-mode-map)
    (define-key map "\C-c\C-d" 'monroe-describe)
    (define-key map "\C-c\C-c" 'monroe-interrupt)
    (define-key map "\M-."     'monroe-jump)
    map))

(defun open-nefia-cs-repl-buffer ()
  "Returns right monroe buffer."
  (get-buffer (format "*open-nefia-cs-repl*")))

(defun open-nefia-cs-repl-process ()
  "Returns right monroe process."
  (get-buffer-process (open-nefia-cs-repl-buffer)))

(defcustom open-nefia-cs-repl-prompt "> "
  "String used for displaying prompt."
  :type 'regexp
  :group 'open-nefia-cs)

(defcustom open-nefia-cs-repl-prompt-regexp "^> *"
  "Regexp to recognize prompts in Monroe more. The same regexp is
used in inferior-lisp."
  :type 'regexp
  :group 'open-nefia-cs)

(defun open-nefia-cs-repl--shell-faces-to-font-lock-faces (text &optional start-pos)
  "Set all 'face in TEXT to 'font-lock-face optionally starting at START-POS."
  (let ((pos 0)
        (start-pos (or start-pos 0))
        next)
    (while (and (/= pos (length text))
                (setq next (next-single-property-change pos 'face text)))
      (let* ((plist (text-properties-at pos text))
             (plist (-if-let (face (plist-get plist 'face))
                        (progn (plist-put plist 'face nil)  ; Swap face
                               (plist-put plist 'font-lock-face face))
                      plist)))
        (set-text-properties (+ start-pos pos) (+ start-pos next) plist)
        (setq pos next)))))

(defun open-nefia-cs-repl--shell-fontify-prompt-post-command-hook ()
  "Fontify just the current line in `hy-shell-buffer' for `post-command-hook'.

Constantly extracts current prompt text and executes and manages applying
`hy--shell-faces-to-font-lock-faces' to the text."
  (-when-let* (((_ . prompt-end) comint-last-prompt)
               (_ (and prompt-end
                       (> (point) prompt-end)  ; new command is being entered
                       (process-live-p (open-nefia-cs-repl-process)))))  ; process alive?
      (let* ((input (buffer-substring-no-properties prompt-end (point-max)))
             (deactivate-mark nil)
             (buffer-undo-list t)
             (text (open-nefia-cs--fontify-str input 'csharp-mode)))
        (open-nefia-cs-repl--shell-faces-to-font-lock-faces text prompt-end))))

(defun open-nefia-cs-repl-input-sender (proc input &optional print-input)
  "Called when user enter data in REPL and when something is received in."
  (if print-input
      (with-current-buffer (open-nefia-cs-repl-buffer)
        (let ((prompt-end (cdr comint-last-prompt)))
          (goto-char prompt-end)
          (insert (format "%s" input))
          (open-nefia-cs-repl--shell-fontify-prompt-post-command-hook)
          (comint-send-input)))
    (open-nefia-cs--send-to-repl input)))

(defvar open-nefia-cs-repl-font-lock-keywords
  (list
   ;; comments
   '(";.*$" . font-lock-comment-face)
   ;; errors
   '("^Error:.*$" . compilation-error)
   '("^   at .*$" . compilation-error)
   ;; output
   '("^.*$" . font-lock-keyword-face))
  "Additional expressions to highlight.")

(define-derived-mode open-nefia-cs-repl-mode comint-mode "Monroe nREPL"
  "Major mode for evaluating commands over nREPL.

The following keys are available in `monroe-mode':

  \\{monroe-mode-map}"

  :syntax-table lisp-mode-syntax-table
  (setq comint-prompt-regexp open-nefia-cs-repl-prompt-regexp)
  (setq comint-input-sender 'open-nefia-cs-repl-input-sender)
  (setq comint-highlight-input nil)
  (setq mode-line-process '(":%s"))
  (setq font-lock-defaults '(open-nefia-cs-repl-font-lock-keywords t))
  (setq truncate-lines nil)
  (add-hook 'post-command-hook
            'open-nefia-cs-repl--shell-fontify-prompt-post-command-hook nil 'local)

  ;; a hack to keep comint happy
  (unless (comint-check-proc (current-buffer))
    (let ((fake-proc (start-process "open-nefia-cs" (current-buffer) nil)))
      (set-process-query-on-exit-flag fake-proc nil)
      (insert (format ";; OpenNefia.NET REPL\n"))
      (set-marker (process-mark fake-proc) (point))
      (comint-output-filter fake-proc open-nefia-cs-repl-prompt))))

;;; user command

;;;###autoload
(defun open-nefia-cs-repl ()
  "Load monroe by setting up appropriate mode, asking user for
connection endpoint."
  (interactive)
  (with-current-buffer
      (get-buffer-create (concat "*open-nefia-cs-repl*"))
    (progn
      (goto-char (point-max))
      (open-nefia-cs-repl-mode)
      (switch-to-buffer (current-buffer)))))

;;;###autoload
(defun open-nefia-cs-validate-prototypes ()
  (interactive)
  (let* ((default-directory (projectile-project-root)))
    (compile "dotnet run --project ./OpenNefia.YAMLValidator/OpenNefia.YAMLValidator.csproj")))

(provide 'open-nefia-cs)
