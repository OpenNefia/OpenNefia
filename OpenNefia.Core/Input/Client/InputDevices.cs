using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.Input
{
    public static class Mouse
    {
        /// <summary>
        ///     Represents one of three mouse buttons.
        /// </summary>
        public enum Button : byte
        {
            Left = 1,
            Middle = 2,
            Right = 3,
            Button4,
            Button5,
            Button6,
            Button7,
            Button8,
            Button9,
            LastButton,
        }

        public static Keyboard.Key MouseButtonToKey(Button button)
        {
            return _mouseKeyMap[button];
        }

        private static readonly Dictionary<Button, Keyboard.Key> _mouseKeyMap = new()
        {
            { Button.Left, Keyboard.Key.MouseLeft },
            { Button.Middle, Keyboard.Key.MouseMiddle },
            { Button.Right, Keyboard.Key.MouseRight },
            { Button.Button4, Keyboard.Key.MouseButton4 },
            { Button.Button5, Keyboard.Key.MouseButton5 },
            { Button.Button6, Keyboard.Key.MouseButton6 },
            { Button.Button7, Keyboard.Key.MouseButton7 },
            { Button.Button8, Keyboard.Key.MouseButton8 },
            { Button.Button9, Keyboard.Key.MouseButton9 },
            { Button.LastButton, Keyboard.Key.Unknown },
        };

    }

    public static class Keyboard
    {
        /// <summary>
        ///     Represents a key on the keyboard.
        /// </summary>
        // This enum HAS to be a byte for the input system bitflag fuckery to work.
        //
        // NOTE: This was changed from Robust to match the KeyConstant enum in Love2dCS.
        // The mouse buttons are added at the end.
        public enum Key : byte
        {
            Unknown,
            Return,
            Enter = Return,
            Escape,
            Backspace,
            Tab,
            Space,
            Exclaim,
            Quotedbl,
            Hash,
            Percent,
            Dollar,
            Ampersand,
            Quote,
            LeftParen,
            RightParen,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Period,
            Slash,

            Number0,
            Number1,
            Number2,
            Number3,
            Number4,
            Number5,
            Number6,
            Number7,
            Number8,
            Number9,

            Colon,
            SemiColon,

            Less,
            Equals,
            Greater,
            Question,
            At,

            LeftBracket,
            Backslash,
            RightBracket,
            Caret,
            Underscore,
            Backquote,

            A,
            B,
            C,
            D,
            E,
            F,
            G,
            H,
            I,
            J,
            K,
            L,
            M,
            N,
            O,
            P,
            Q,
            R,
            S,
            T,
            U,
            V,
            W,
            X,
            Y,
            Z,

            CapsLock,

            F1,
            F2,
            F3,
            F4,
            F5,
            F6,
            F7,
            F8,
            F9,
            F10,
            F11,
            F12,

            PrintScreen,
            ScrollLock,

            Pause,
            Insert,
            Home,
            PageUp,
            Delete,
            End,
            PageDown,
            Right,
            Left,
            Down,
            Up,

            NumLockClear,
            KeypadDivide,
            KeypadMultiply,
            KeypadMinus,
            KeypadPlus,
            KeypadEnter,
            Keypad1,
            Keypad2,
            Keypad3,
            Keypad4,
            Keypad5,
            Keypad6,
            Keypad7,
            Keypad8,
            Keypad9,
            Keypad0,
            KeypadPeriod,
            KeypadComma,
            KeypadEquals,

            Application,
            Power,

            F13,
            F14,
            F15,
            F16,
            F17,
            F18,
            F19,
            F20,
            F21,
            F22,
            F23,
            F24,

            Execute,
            Help,
            Menu,
            Select,
            Stop,
            Again,
            Undo,
            Cut,
            Copy,
            Paste,
            Find,
            Mute,

            VolumeUp,
            VolumeDown,
            Alterase,

            Sysreq,
            Cancel,
            Clear,
            Prior,
            Return2,
            Separator,
            Out,
            Oper,

            ClearAgain,

            ThousandsSeparator,
            DecimalSeparator,
            CurrencyUnit,
            CurrencySubunit,

            LCtrl,
            LShift,
            LAlt,
            LGUI,
            RCtrl,
            RShift,
            RAlt,
            RGUI,

            Mode,

            AudioNext,
            AudioPrev,
            AudioStop,
            AudioPlay,
            AudioMute,
            MediaSelect,

            WWW,
            Mail,
            Calculator,
            Computer,
            AppSearch,
            AppHome,
            AppBack,
            AppForward,
            AppStop,
            AppRefresh,
            AppBookmarks,

            BrightnessDown,
            BrightnessUp,
            DisplaySwitch,
            KBDILLUMToggle,
            KBDILLUMDown,
            KBDILLUMUp,
            Eject,
            Sleep,

            // NOTE: These extend beyond Love.KeyConstant!
            MouseLeft,
            MouseRight,
            MouseMiddle,
            MouseButton4,
            MouseButton5,
            MouseButton6,
            MouseButton7,
            MouseButton8,
            MouseButton9,
        }

        public static bool IsMouseKey(this Key key)
        {
            return key >= Key.MouseLeft && key <= Key.MouseButton9;
        }

        /// <summary>
        ///     Gets a "nice" version of special unprintable keys such as <see cref="Key.Escape"/>.
        /// </summary>
        /// <returns><see langword="null"/> if there is no nice version of this special key.</returns>
        internal static string? GetSpecialKeyName(Key key, ILocalizationManager loc)
        {
            var locId = $"input-key-{key}";
            if (key == Key.LGUI || key == Key.RGUI)
            {
                if (OperatingSystem.IsWindows())
                    locId += "-win";
                else if (OperatingSystem.IsMacOS())
                    locId += "-mac";
                else
                    locId += "-linux";
            }

            if (loc.TryGetString(locId, out var name))
                return name;

            return loc.GetString("input-key-unknown");
        }
    }
}
