
using OpenNefia.Core.Input;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.UI.Element
{
    public partial class UiElement
    {
        /// <summary>
        ///     Invoked when the mouse enters the area of this control / when it hovers over the control.
        /// </summary>
        public event Action<GUIMouseHoverEventArgs>? OnMouseEntered;

        protected internal virtual void MouseEntered()
        {
            OnMouseEntered?.Invoke(new GUIMouseHoverEventArgs(this));
        }

        /// <summary>
        ///     Invoked when the mouse exits the area of this control / when it stops hovering over the control.
        /// </summary>
        public event Action<GUIMouseHoverEventArgs>? OnMouseExited;

        protected internal virtual void MouseExited()
        {
            OnMouseExited?.Invoke(new GUIMouseHoverEventArgs(this));
        }

        protected internal virtual void MouseWheel(GUIMouseWheelEventArgs args)
        {
        }

        public event Action<GUIBoundKeyEventArgs>? OnKeyBindDown;
        public event Action<GUIBoundKeyEventArgs>? OnKeyBindUp;
        public event Action<GUIMouseMoveEventArgs>? OnMouseMoved;

        protected internal virtual void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            OnKeyBindDown?.Invoke(args);
        }

        protected internal virtual void KeyBindUp(GUIBoundKeyEventArgs args)
        {
            OnKeyBindUp?.Invoke(args);
        }

        protected internal virtual void MouseMove(GUIMouseMoveEventArgs args)
        {
            OnMouseMoved?.Invoke(args);
        }

        protected internal virtual void KeyHeld(GUIKeyEventArgs args)
        {
        }

        protected internal virtual void TextEntered(GUITextEventArgs args)
        {
        }

        public event Action<GUIScaleChangedEventArgs>? OnUIScaleChanged;

        protected internal virtual void UIScaleChanged(GUIScaleChangedEventArgs args)
        {
            OnUIScaleChanged?.Invoke(args);
        }
    }

    public class GUIMouseHoverEventArgs : EventArgs
    {
        /// <summary>
        ///     The control this event originated from.
        /// </summary>
        public UiElement SourceControl { get; }

        public GUIMouseHoverEventArgs(UiElement sourceControl)
        {
            SourceControl = sourceControl;
        }
    }

    public class GUIBoundKeyEventArgs : BoundKeyEventArgs
    {
        /// <summary>
        ///     Position of the mouse, relative to the current control, in virtual pixels.
        /// </summary>
        public Vector2 RelativePosition { get; internal set; }

        /// <summary>
        ///     Position of the mouse, relative to the current control.
        /// </summary>
        public Vector2 RelativePixelPosition { get; internal set; }

        public GUIBoundKeyEventArgs(BoundKeyFunction function, BoundKeyState state, ScreenCoordinates pointerLocation,
            bool canFocus, Vector2 relativePosition, Vector2 relativePixelPosition)
            : base(function, state, pointerLocation, canFocus)
        {
            RelativePixelPosition = relativePixelPosition;
            RelativePosition = relativePosition;
        }
    }

    public class GUIKeyEventArgs : KeyEventArgs
    {
        /// <summary>
        ///     The control spawning this event.
        /// </summary>
        public UiElement SourceControl { get; }

        public GUIKeyEventArgs(UiElement sourceControl,
            Keyboard.Key key,
            bool repeat,
            bool alt,
            bool control,
            bool shift,
            bool system,
            Love.Scancode scanCode)
            : base(key, repeat, alt, control, shift, system, scanCode)
        {
            SourceControl = sourceControl;
        }
    }

    public class GUITextEventArgs : TextEventArgs
    {
        /// <summary>
        ///     The control spawning this event.
        /// </summary>
        public UiElement SourceControl { get; }

        public GUITextEventArgs(UiElement sourceControl,
            uint codePoint)
            : base(codePoint)
        {
            SourceControl = sourceControl;
        }
    }

    public abstract class GUIMouseEventArgs : InputEventArgs
    {
        /// <summary>
        ///     The control spawning this event.
        /// </summary>
        public UiElement SourceControl { get; internal set; }

        /// <summary>
        ///     Position of the mouse, relative to the screen.
        /// </summary>
        public Vector2 GlobalPosition { get; }

        public ScreenCoordinates GlobalPixelPosition { get; }

        /// <summary>
        ///     Position of the mouse, relative to the current control.
        /// </summary>
        public Vector2 RelativePosition { get; internal set; }

        public Vector2 RelativePixelPosition { get; internal set; }

        protected GUIMouseEventArgs(UiElement sourceControl,
            Vector2 globalPosition,
            ScreenCoordinates globalPixelPosition,
            Vector2 relativePosition,
            Vector2 relativePixelPosition)
        {
            SourceControl = sourceControl;
            GlobalPosition = globalPosition;
            RelativePosition = relativePosition;
            RelativePixelPosition = relativePixelPosition;
            GlobalPixelPosition = globalPixelPosition;
        }
    }

    public class GUIMouseMoveEventArgs : GUIMouseEventArgs
    {
        /// <summary>
        ///     The new position relative to the previous position, in virtual pixels.
        /// </summary>
        public Vector2 Relative { get; }

        // ALL the parameters!
        public GUIMouseMoveEventArgs(Vector2 relative,
            UiElement sourceElement,
            Vector2 globalPosition,
            ScreenCoordinates globalPixelPosition,
            Vector2 relativePosition,
            Vector2 relativePixelPosition)
            : base(sourceElement, globalPosition, globalPixelPosition, relativePosition, relativePixelPosition)
        {
            Relative = relative;
        }
    }

    public class GUIMouseWheelEventArgs : GUIMouseEventArgs
    {
        public Vector2 Delta { get; }

        public GUIMouseWheelEventArgs(Vector2 delta,
            UiElement sourceElement,
            Vector2 globalPosition,
            ScreenCoordinates globalPixelPosition,
            Vector2 relativePosition,
            Vector2 relativePixelPosition)
            : base(sourceElement, globalPosition, globalPixelPosition, relativePosition, relativePixelPosition)
        {
            Delta = delta;
        }
    }

    public class GUIScaleChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     The new global UI scale.
        /// </summary>
        public float GlobalUIScale { get; }

        public GUIScaleChangedEventArgs(float globalUIScale)
        {
            GlobalUIScale = globalUIScale;
        }
    }
}
