using OpenNefia.Core.Maths;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.ViewVariables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Wisp.Controls

{    /// <summary>
     ///     Allows the user to input and modify a line of text.
     /// </summary>
    public class LineEdit : WispControl
    {
        private const float BlinkTime = 0.5f;
        private const float MouseScrollDelay = 0.001f;

        public const string StylePropertyStyleBox = "stylebox";
        public const string StylePropertyCursorColor = "cursor-color";
        public const string StylePropertySelectionColor = "selection-color";
        public const string StyleClassLineEditNotEditable = "notEditable";
        public const string StylePseudoClassPlaceholder = "placeholder";

        // It is assumed that these two positions are NEVER inside a surrogate pair in the text buffer.
        private int _cursorPosition;
        private int _selectionStart;
        private string _text = "";
        private bool _editable = true;
        private string? _placeHolder;

        private int _drawOffset;

        private float _cursorBlinkTimer;
        private bool _cursorCurrentlyLit;
        // private readonly LineEditRenderBox _renderBox;

        private bool _mouseSelectingText;
        private float _lastMousePosition;

        private bool IsPlaceHolderVisible => string.IsNullOrEmpty(_text) && _placeHolder != null;

        public event Action<LineEditEventArgs>? OnTextChanged;
        public event Action<LineEditEventArgs>? OnTextEntered;
        public event Action<LineEditEventArgs>? OnFocusEnter;
        public event Action<LineEditEventArgs>? OnFocusExit;
        public event Action<LineEditEventArgs>? OnTabComplete;

        /// <summary>
        ///     Determines whether the LineEdit text gets changed by the input text.
        /// </summary>
        public Func<string, bool>? IsValid { get; set; }

        /// <summary>
        ///     The actual text currently stored in the LineEdit.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public string Text
        {
            get => _text;
            set
            {
                if (value == null)
                {
                    value = "";
                }

                if (!SetText(value))
                {
                    return;
                }

                _cursorPosition = 0;
                _selectionStart = 0;
                // _updatePseudoClass();
            }
        }

        /// <summary>
        ///     The text
        /// </summary>
        public ReadOnlySpan<char> SelectedText
        {
            get
            {
                var lower = SelectionLower;

                return _text.AsSpan(lower, SelectionLength);
            }
        }

        public int SelectionLength => Math.Abs(_selectionStart - _cursorPosition);

        [ViewVariables(VVAccess.ReadWrite)]
        public bool Editable
        {
            get => _editable;
            set
            {
                _editable = value;
                if (_editable)
                {
                    // DefaultCursorShape = CursorShape.IBeam;
                    RemoveStyleClass(StyleClassLineEditNotEditable);
                }
                else
                {
                    // DefaultCursorShape = CursorShape.Arrow;
                    AddStyleClass(StyleClassLineEditNotEditable);
                }
            }
        }

        [ViewVariables(VVAccess.ReadWrite)]
        public string? PlaceHolder
        {
            get => _placeHolder;
            set
            {
                _placeHolder = value;
                // _updatePseudoClass();
            }
        }

        public int CursorPosition
        {
            get => _cursorPosition;
            set
            {
                var clamped = MathHelper.Clamp(value, 0, _text.Length);
                if (_text.Length != 0 && _text.Length != clamped && !Rune.TryGetRuneAt(_text, clamped, out _))
                    throw new ArgumentException("Cannot set cursor inside surrogate pair.");

                _cursorPosition = clamped;
                _selectionStart = _cursorPosition;
            }
        }

        public int SelectionStart
        {
            get => _selectionStart;
            set
            {
                var clamped = MathHelper.Clamp(value, 0, _text.Length);
                if (_text.Length != 0 && _text.Length != clamped && !Rune.TryGetRuneAt(_text, clamped, out _))
                    throw new ArgumentException("Cannot set cursor inside surrogate pair.");

                _selectionStart = clamped;
            }
        }

        public int SelectionLower => Math.Min(_selectionStart, _cursorPosition);
        public int SelectionUpper => Math.Max(_selectionStart, _cursorPosition);

        public bool IgnoreNext { get; set; }

        public LineEdit()
        {
            EventFilter = UIEventFilterMode.Stop;
            CanKeyboardFocus = true;
            KeyboardFocusOnClick = true;

            // DefaultCursorShape = CursorShape.IBeam;

            // AddChild(_renderBox = new LineEditRenderBox(this));
        }

        public void Clear()
        {
            Text = "";
        }

        public void InsertAtCursor(string text)
        {
            /*
            // Strip newlines.
            var chars = new List<char>(text.Length);
            foreach (var chr in text)
            {
                if (chr == '\n')
                {
                    continue;
                }

                chars.Add(chr);
            }

            text = new string(chars.ToArray());

            var lower = SelectionLower;
            var newContents = Text[..lower] + text + Text[SelectionUpper..];

            if (!SetText(newContents))
            {
                return;
            }

            _selectionStart = _cursorPosition = lower + chars.Count;
            OnTextChanged?.Invoke(new LineEditEventArgs(this, _text));
            _updatePseudoClass();
            */
        }

        /// <remarks>
        /// Does not fix cursor positions, those will have to be adjusted manually.
        /// </remarks>>
        protected bool SetText(string newText)
        {
            if (IsValid != null && !IsValid(newText))
            {
                return false;
            }

            _text = newText;
            return true;
        }






        public sealed class LineEditEventArgs : EventArgs
        {
            public LineEdit Control { get; }
            public string Text { get; }

            public LineEditEventArgs(LineEdit control, string text)
            {
                 Control = control;
               Text = text;
            }
        }








    }
}
