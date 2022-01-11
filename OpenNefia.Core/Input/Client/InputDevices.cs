using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static OpenNefia.Core.Input.Mouse;

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

    public static class Gamepad
    {
        /// <summary>
        /// Virtual gamepad buttons.
        /// </summary>
        /// <remarks>
        /// Same as <see cref="Love.GamepadButton"/>.
        /// </remarks>
        public enum Button
        {
            A = 1,
            B,
            X,
            Y,
            Back,
            Guide,
            Start,

            /// <summary>
            /// Left stick click button.
            /// </summary>
            LeftStick,

            /// <summary>
            /// Right stick click button.
            /// </summary>
            RightStick,

            /// <summary>
            /// Left bumper.
            /// </summary>
            LeftShoulder,

            /// <summary>
            /// Right bumper.
            /// </summary>
            RightShoulder,

            /// <summary>
            /// D-pad up.
            /// </summary>
            DPadUp,

            /// <summary>
            /// D-pad down.
            /// </summary>
            DPadDown,

            /// <summary>
            /// D-pad left.
            /// </summary>
            DPadLeft,

            /// <summary>
            /// D-pad right.
            /// </summary>
            DPadRight,
        };

        /// <summary>
        /// Virtual gamepad axes.
        /// </summary>
        /// <remarks>
        /// Same as <see cref="Love.GamepadAxis"/>.
        /// </remarks>
        public enum Axis
        {
            LeftX = 1,
            LeftY,
            RightX,
            RightY,
            TriggerLeft,
            TriggerRight,
        };

        public static Keyboard.Key GamepadButtonToKey(Button button)
        {
            return _gamepadKeyMap[button];
        }

        public static Keyboard.Key GamepadAxisToKey(Axis axis, float value)
        {
            if (value < 0)
                return _gamepadAxisMinusMap[axis];
            else
                return _gamepadAxisPlusMap[axis];
        }

        private static readonly Dictionary<Button, Keyboard.Key> _gamepadKeyMap = new()
        {
            { Button.A, Keyboard.Key.GamepadA },
            { Button.B, Keyboard.Key.GamepadB },
            { Button.X, Keyboard.Key.GamepadX },
            { Button.Y, Keyboard.Key.GamepadY },
            { Button.Back, Keyboard.Key.GamepadBack },
            { Button.Guide, Keyboard.Key.GamepadGuide },
            { Button.Start, Keyboard.Key.GamepadStart },
            { Button.LeftStick, Keyboard.Key.GamepadLeftStick },
            { Button.RightStick, Keyboard.Key.GamepadRightStick },
            { Button.LeftShoulder, Keyboard.Key.GamepadLeftShoulder },
            { Button.RightShoulder, Keyboard.Key.GamepadRightShoulder },
            { Button.DPadUp, Keyboard.Key.GamepadDPadUp },
            { Button.DPadDown, Keyboard.Key.GamepadDPadDown },
            { Button.DPadLeft, Keyboard.Key.GamepadDPadLeft },
            { Button.DPadRight, Keyboard.Key.GamepadDPadRight },
        };

        private static readonly Dictionary<Axis, Keyboard.Key> _gamepadAxisMinusMap = new()
        {
            { Axis.LeftX, Keyboard.Key.GamepadAxisLeftXMinus },
            { Axis.LeftY, Keyboard.Key.GamepadAxisLeftYMinus },
            { Axis.RightX, Keyboard.Key.GamepadAxisRightXMinus },
            { Axis.RightY, Keyboard.Key.GamepadAxisRightYMinus },
            { Axis.TriggerLeft, Keyboard.Key.GamepadAxisTriggerLeftMinus },
            { Axis.TriggerRight, Keyboard.Key.GamepadAxisTriggerRightMinus },
        };

        private static readonly Dictionary<Axis, Keyboard.Key> _gamepadAxisPlusMap = new()
        {
            { Axis.LeftX, Keyboard.Key.GamepadAxisLeftXPlus },
            { Axis.LeftY, Keyboard.Key.GamepadAxisLeftYPlus },
            { Axis.RightX, Keyboard.Key.GamepadAxisRightXPlus },
            { Axis.RightY, Keyboard.Key.GamepadAxisRightYPlus },
            { Axis.TriggerLeft, Keyboard.Key.GamepadAxisTriggerLeftPlus },
            { Axis.TriggerRight, Keyboard.Key.GamepadAxisTriggerRightPlus },
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

            GamepadA,
            GamepadB,
            GamepadX,
            GamepadY,
            GamepadBack,
            GamepadGuide,
            GamepadStart,
            GamepadLeftStick,
            GamepadRightStick,
            GamepadLeftShoulder,
            GamepadRightShoulder,
            GamepadDPadUp,
            GamepadDPadDown,
            GamepadDPadLeft,
            GamepadDPadRight,

            GamepadAxisLeftXMinus,
            GamepadAxisLeftXPlus,
            GamepadAxisLeftYMinus,
            GamepadAxisLeftYPlus,
            GamepadAxisRightXMinus,
            GamepadAxisRightXPlus,
            GamepadAxisRightYMinus,
            GamepadAxisRightYPlus,
            GamepadAxisTriggerLeftMinus,
            GamepadAxisTriggerLeftPlus,
            GamepadAxisTriggerRightMinus,
            GamepadAxisTriggerRightPlus,
        }

        public static bool IsMouseKey(this Key key)
        {
            return key >= Key.MouseLeft && key <= Key.MouseButton9;
        }

        /// <summary>
        ///     Gets a "nice" version of special unprintable keys such as <see cref="Key.Escape"/>.
        /// </summary>
        /// <returns><see langword="false"/> if there is no nice version of this special key.</returns>
        internal static bool TryGetSpecialKeyName(Key key, ILocalizationManager loc, [NotNullWhen(true)] out string? name)
        {
            string keyName;
            switch (key)
            {
                case Key.LCtrl:
                case Key.RCtrl:
                    keyName = "Control";
                    break;
                case Key.LAlt:
                case Key.RAlt:
                    keyName = "Alt";
                    break;
                case Key.LShift:
                case Key.RShift:
                    keyName = "Shift";
                    break;
                case Key.LGUI:
                case Key.RGUI:
                    keyName = key.ToString();
                    if (OperatingSystem.IsWindows())
                        keyName += "_Win";
                    else if (OperatingSystem.IsMacOS())
                        keyName += "_Mac";
                    else
                        keyName += "_Linux";
                    break;
                default:
                    keyName = key.ToString();
                    break;
            }

            var locId = $"OpenNefia.Input.Keys.{keyName}";
            return loc.TryGetString(locId, out name);
        }
    }
}
