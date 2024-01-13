using System;
using System.Text;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using Scancode = Love.Scancode;

namespace OpenNefia.Core.Input
{
    public abstract class InputEventArgs : EventArgs
    {
        public bool Handled { get; private set; }

        /// <summary>
        ///     Mark this event as handled.
        /// </summary>
        public void Handle()
        {
            Handled = true;
        }
    }

    /// <summary>
    ///     Generic input event that has modifier keys like control.
    /// </summary>
    public abstract class ModifierInputEventArgs : InputEventArgs
    {
        /// <summary>
        ///     Whether the alt key (⌥ Option on MacOS) is held.
        /// </summary>
        public bool Alt { get; }

        /// <summary>
        ///     Whether the control key is held.
        /// </summary>
        public bool Control { get; }

        /// <summary>
        ///     Whether the shift key is held.
        /// </summary>
        public bool Shift { get; }

        /// <summary>
        ///     Whether the system key (Windows key, ⌘ Command on MacOS) is held.
        /// </summary>
        public bool System { get; }

        protected ModifierInputEventArgs(bool alt, bool control, bool shift, bool system)
        {
            Alt = alt;
            Control = control;
            Shift = shift;
            System = system;
        }
    }

    public class TextEventArgs : EventArgs
    {
        public TextEventArgs(uint codePoint)
        {
            CodePoint = codePoint;
        }

        public TextEventArgs(string text)
        {
            CodePoint = (uint)Rune.GetRuneAt(text, 0).Value;
        }

        public uint CodePoint { get; }
        public Rune AsRune => new Rune(CodePoint);
    }

    public class TextEditingEventArgs : EventArgs
    {
        public TextEditingEventArgs(string text, int start, int end)
        {
            Text = text;
            Start = start;
            End = end;
        }

        public string Text { get; }
        public int Start { get; }
        public int End { get; }
    }

    public class KeyEventArgs : ModifierInputEventArgs
    {
        /// <summary>
        ///     The key that got pressed or released.
        /// </summary>
        public Keyboard.Key Key { get; }

        /// <summary>
        ///     If true, this key is being held down and another key event is being fired by the OS.
        /// </summary>
        public bool IsRepeat { get; }

        public Scancode ScanCode { get; }

        public KeyEventArgs(
            Keyboard.Key key,
            bool repeat,
            bool alt, bool control, bool shift, bool system,
            Scancode scanCode)
            : base(alt, control, shift, system)
        {
            Key = key;
            IsRepeat = repeat;
            ScanCode = scanCode;
        }
    }

    public abstract class MouseEventArgs : InputEventArgs
    {
        /// <summary>
        ///     Position of the mouse relative to the screen.
        /// </summary>
        public ScreenCoordinates Position { get; }

        /// <summary>
        ///     Whether this input event was a touch input.
        /// </summary>
        public bool IsTouch { get; }

        protected MouseEventArgs(ScreenCoordinates position, bool isTouch)
        {
            Position = position;
            IsTouch = isTouch;
        }
    }

    public class MouseButtonEventArgs : MouseEventArgs
    {
        /// <summary>
        ///     The mouse button that has been pressed or released.
        /// </summary>
        public Mouse.Button Button { get; }

        // ALL the parameters!
        public MouseButtonEventArgs(ScreenCoordinates position, Mouse.Button button, bool isTouch)
            : base(position, isTouch)
        {
            Button = button;
        }
    }

    public class MouseWheelEventArgs : MouseEventArgs
    {
        /// <summary>
        ///     The direction the mouse wheel was moved in.
        /// </summary>
        public Vector2 Delta { get; }

        // ALL the parameters!
        public MouseWheelEventArgs(ScreenCoordinates position, Vector2 delta)
            : base(position, false)
        {
            Delta = delta;
        }
    }

    public class MouseMoveEventArgs : MouseEventArgs
    {
        /// <summary>
        ///     The new position relative to the previous position.
        /// </summary>
        public Vector2 Relative { get; }

        // ALL the parameters!
        public MouseMoveEventArgs(ScreenCoordinates position, Vector2 relative, bool isTouch)
            : base(position, isTouch)
        {
            Relative = relative;
        }
    }

    public class MouseEnterLeaveEventArgs : EventArgs
    {
        /// <summary>
        ///     True if the mouse ENTERED the window, false if it LEFT the window.
        /// </summary>
        public bool Entered { get; }

        // ALL the parameters!
        public MouseEnterLeaveEventArgs(bool entered)
        {
            Entered = entered;
        }
    }

    public class WindowScaleChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     The new global UI scale.
        /// </summary>
        public float UIScale { get; }

        public WindowScaleChangedEventArgs(float uiScale)
        {
            UIScale = uiScale;
        }
    }

    public class FontHeightScaleChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     The new font height scale.
        /// </summary>
        public float FontHeightScale { get; }

        public FontHeightScaleChangedEventArgs(float fontHeightScale)
        {
            FontHeightScale = fontHeightScale;
        }
    }
}
