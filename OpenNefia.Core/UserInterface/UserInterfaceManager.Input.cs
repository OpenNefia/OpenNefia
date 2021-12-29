using OpenNefia.Core.Input;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UserInterface
{
    public sealed partial class UserInterfaceManager
    {
        /// <inheritdoc/>
        public bool HandleCanFocusDown(ScreenCoordinates pointerPosition, [NotNullWhen(true)] out (UiElement control, Vector2i rel)? hitData)
        {
            var hit = MouseGetControlAndRel(pointerPosition);

            ReleaseKeyboardFocus();

            if (hit == null)
            {
                hitData = null;
                return false;
            }

            var (control, rel) = hit.Value;

            ControlFocused = control;

            if (ControlFocused.CanKeyboardFocus && ControlFocused.KeyboardFocusOnClick)
            {
                ControlFocused.GrabKeyboardFocus();
            }

            hitData = (control, (Vector2i)rel);
            return true;
        }

        /// <inheritdoc/>
        public void HandleCanFocusUp()
        {
            ControlFocused = null;
        }

        public void KeyBindDown(BoundKeyEventArgs args)
        {
            var control = ControlFocused ?? KeyboardFocused ?? MouseGetControl(args.PointerLocation);

            if (control == null)
            {
                return;
            }

            var guiArgs = new GUIBoundKeyEventArgs(args.Function, args.State, args.PointerLocation, args.CanFocus,
                args.PointerLocation.Position - control.GlobalPixelPosition);

            _doGuiInput(control, guiArgs, (c, ev) => c.KeyBindDown(ev));

            if (guiArgs.Handled)
            {
                args.Handle();
            }
        }

        public void KeyBindUp(BoundKeyEventArgs args)
        {
            var control = ControlFocused ?? KeyboardFocused ?? MouseGetControl(args.PointerLocation);
            if (control == null)
            {
                return;
            }

            var guiArgs = new GUIBoundKeyEventArgs(args.Function, args.State, args.PointerLocation, args.CanFocus,
                args.PointerLocation.Position - control.GlobalPixelPosition);

            _doGuiInput(control, guiArgs, (c, ev) => c.KeyBindUp(ev));

            // Always mark this as handled.
            // The only case it should not be is if we do not have a control to click on,
            // in which case we never reach this.
            args.Handle();
        }

        public void MouseMove(MouseMoveEventArgs mouseMoveEventArgs)
        {
            // Update which control is considered hovered.
            var newHovered = MouseGetControl(mouseMoveEventArgs.Position);
            if (newHovered != CurrentlyHovered)
            {
                CurrentlyHovered?.MouseExited();
                CurrentlyHovered = newHovered;
                CurrentlyHovered?.MouseEntered();
            }

            var target = ControlFocused ?? newHovered;
            if (target != null)
            {
                var pos = mouseMoveEventArgs.Position.Position;
                var guiArgs = new GUIMouseMoveEventArgs(mouseMoveEventArgs.Relative,
                    target,
                    mouseMoveEventArgs.Position,
                    pos - target.GlobalPixelPosition);

                _doMouseGuiInput(target, guiArgs, (c, ev) => c.MouseMove(ev));
            }
        }

        public void MouseWheel(MouseWheelEventArgs args)
        {
            var control = MouseGetControl(args.Position);
            if (control == null)
            {
                return;
            }

            args.Handle();

            var pos = args.Position.Position;

            var guiArgs = new GUIMouseWheelEventArgs(args.Delta, control,
                args.Position,
                pos - control.GlobalPixelPosition);

            _doMouseGuiInput(control, guiArgs, (c, ev) => c.MouseWheel(ev), true);
        }

        public void TextEntered(TextEventArgs textEvent)
        {
            if (KeyboardFocused == null)
            {
                return;
            }

            var guiArgs = new GUITextEventArgs(KeyboardFocused, textEvent.CodePoint);
            KeyboardFocused.TextEntered(guiArgs);
        }

        private static void _doMouseGuiInput<T>(UiElement? control, T guiEvent, Action<UiElement, T> action,
            bool ignoreStop = false)
            where T : GUIMouseEventArgs
        {
            while (control != null)
            {
                guiEvent.RelativePixelPosition = guiEvent.GlobalPixelPosition.Position - control.GlobalPixelPosition;

                guiEvent.SourceControl = control;
                if (control.MouseFilter != MouseFilterMode.Ignore)
                {
                    action(control, guiEvent);

                    if (guiEvent.Handled || (!ignoreStop && control.MouseFilter == MouseFilterMode.Stop))
                    {
                        break;
                    }
                }

                control = control.Parent;
            }
        }

        private static void _doGuiInput<T>(UiElement? control, T guiEvent, Action<UiElement, T> action,
            bool ignoreStop = false)
            where T : GUIBoundKeyEventArgs
        {
            while (control != null)
            {
                guiEvent.RelativePixelPosition = guiEvent.PointerLocation.Position - control.GlobalPixelPosition;

                if (control.MouseFilter != MouseFilterMode.Ignore)
                {
                    action(control, guiEvent);

                    if (guiEvent.Handled || (!ignoreStop && control.MouseFilter == MouseFilterMode.Stop))
                    {
                        break;
                    }
                }

                control = control.Parent;
            }
        }
    }
}
