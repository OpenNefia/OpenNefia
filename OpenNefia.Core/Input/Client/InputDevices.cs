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
            Left = 0,
            Middle = 1,
            Right = 2,
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

            // These were renamed from Keypad<...> to Numpad<...>.
            NumLockClear,
            NumpadDivide,
            NumpadMultiply,
            NumpadMinus,
            NumpadPlus,
            NumpadEnter,
            Numpad1,
            Numpad2,
            Numpad3,
            Numpad4,
            Numpad5,
            Numpad6,
            Numpad7,
            Numpad8,
            Numpad9,
            Numpad0,
            NumpadPeriod,
            NumpadComma,
            NumpadEquals,

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

            Control,
            Shift,
            Alt,
            GUI,
            LCtrl = Control,
            LShift = Shift,
            LAlt = Alt,
            LGUI = GUI,
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

            JoystickButton1,
            JoystickButton2,
            JoystickButton3,
            JoystickButton4,
            JoystickButton5,
            JoystickButton6,
            JoystickButton7,
            JoystickButton8,
            JoystickButton9,
            JoystickButton10,
            JoystickButton11,
            JoystickButton12,
            JoystickButton13,
            JoystickButton14,
            JoystickButton15,
            JoystickButton16,
            JoystickButton17,
            JoystickButton18,
            JoystickButton19,
            JoystickButton20,

            JoystickAxis1Minus,
            JoystickAxis1Plus,
            JoystickAxis2Minus,
            JoystickAxis2Plus,
            JoystickAxis3Minus,
            JoystickAxis3Plus,
            JoystickAxis4Minus,
            JoystickAxis4Plus,

            JoystickHat1Up,
            JoystickHat1Down,
            JoystickHat1Left,
            JoystickHat1Right,
            JoystickHat2Up,
            JoystickHat2Down,
            JoystickHat2Left,
            JoystickHat2Right,
        }

        /// <summary>
        /// Joystick hat values.
        /// </summary>
        /// <remarks>
        /// Same as <see cref="Love.JoystickHat"/>.
        /// </remarks>
        public enum JoystickHat
        {
            Centered = 1,
            Up,
            Right,
            Down,
            Left,

            /// <summary>
            /// Right+Up
            /// </summary>
            RightUp,

            /// <summary>
            /// Right+Down
            /// </summary>
            RightDown,

            /// <summary>
            /// Left+Up
            /// </summary>
            LeftUp,

            /// <summary>
            /// Left+Down
            /// </summary>
            LeftDown,
        };

        public static bool IsMouseKey(this Key key)
        {
            return key >= Key.MouseLeft && key <= Key.MouseButton9;
        }

        public static Key JoystickButtonToKey(int button)
        {
            if (button < 1 || button > 20)
                return Key.Unknown;

            return (Key)((byte)Key.JoystickButton1 + ((byte)button - 1));
        }

        public static Key JoystickAxisToKey(int axis, float value)
        {
            switch (axis)
            {
                case 1:
                    if (value < 0)
                        return Key.JoystickAxis1Minus;
                    else
                        return Key.JoystickAxis1Plus;
                case 2:
                    if (value < 0)
                        return Key.JoystickAxis2Minus;
                    else
                        return Key.JoystickAxis2Plus;
                case 3:
                    if (value < 0)
                        return Key.JoystickAxis3Minus;
                    else
                        return Key.JoystickAxis3Plus;
                case 4:
                    if (value < 0)
                        return Key.JoystickAxis4Minus;
                    else
                        return Key.JoystickAxis4Plus;
            }

            return Key.Unknown;
        }

        // NOTE: Assumes no centered/diagonals.
        public static Key JoystickHatToKey(int hat, JoystickHat direction)
        {
            switch (hat)
            {
                case 1:
                    switch (direction)
                    {
                        case JoystickHat.Up:
                            return Key.JoystickHat1Up;
                        case JoystickHat.Down:
                            return Key.JoystickHat1Down;
                        case JoystickHat.Left:
                            return Key.JoystickHat1Left;
                        case JoystickHat.Right:
                            return Key.JoystickHat1Right;
                    }
                    break;
                case 2:
                    switch (direction)
                    {
                        case JoystickHat.Up:
                            return Key.JoystickHat2Up;
                        case JoystickHat.Down:
                            return Key.JoystickHat2Down;
                        case JoystickHat.Left:
                            return Key.JoystickHat2Left;
                        case JoystickHat.Right:
                            return Key.JoystickHat2Right;
                    }
                    break;
            }

            return Key.Unknown;
        }

        /// <summary>
        /// Gets the pressed/released key diff between two joystick hat states.
        /// </summary>
        /// <remarks>
        /// Necessary since LÖVE doesn't give us pressed/released states for joystick hats.
        /// </remarks>
        private static IEnumerable<JoystickHat> DiffJoystickHats(JoystickHat prevState, JoystickHat nextState)
        {
            var prevUp = prevState == JoystickHat.Up || prevState == JoystickHat.LeftUp || prevState == JoystickHat.RightUp;
            var prevDown = prevState == JoystickHat.Down || prevState == JoystickHat.LeftDown || prevState == JoystickHat.RightDown;
            var prevLeft = prevState == JoystickHat.Left || prevState == JoystickHat.LeftUp || prevState == JoystickHat.LeftDown;
            var prevRight = prevState == JoystickHat.Right || prevState == JoystickHat.RightDown || prevState == JoystickHat.RightDown;

            var nextUp = nextState == JoystickHat.Up || nextState == JoystickHat.LeftUp || nextState == JoystickHat.RightUp;
            var nextDown = nextState == JoystickHat.Down || nextState == JoystickHat.LeftDown || nextState == JoystickHat.RightDown;
            var nextLeft = nextState == JoystickHat.Left || nextState == JoystickHat.LeftUp || nextState == JoystickHat.LeftDown;
            var nextRight = nextState == JoystickHat.Right || nextState == JoystickHat.RightDown || nextState == JoystickHat.RightDown;

            if (!prevUp && nextUp)
                yield return JoystickHat.Up;
            if (!prevDown && nextDown)
                yield return JoystickHat.Down;
            if (!prevLeft && nextLeft)
                yield return JoystickHat.Left;
            if (!prevRight && nextRight)
                yield return JoystickHat.Right;
        }

        internal static IEnumerable<Key> GetPressedHatKeys(int hat, JoystickHat lastHatState, JoystickHat newHatState)
        {
            return DiffJoystickHats(lastHatState, newHatState)
                .Select(dir => JoystickHatToKey(hat, dir));
        }

        internal static IEnumerable<Key> GetReleasedHatKeys(int hat, JoystickHat lastHatState, JoystickHat newHatState)
        {
            return DiffJoystickHats(newHatState, lastHatState)
                .Select(dir => JoystickHatToKey(hat, dir));
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
