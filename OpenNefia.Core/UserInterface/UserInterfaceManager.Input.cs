using OpenNefia.Core.Input;
using OpenNefia.Core.Log;
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

            if (hit == null)
            {
                hitData = null;
                return false;
            }

            var (control, rel) = hit.Value;

            if (control.CanControlFocus)
            {
                Logger.DebugS("ui.input", $"FOCUS: {control}");
                ControlFocused = control;
            }

            if (control.CanKeyboardFocus && control.KeyboardFocusOnClick)
            {
                Logger.DebugS("ui.input", $"KEYBOARD FOCUS: {control}");
                control.GrabKeyboardFocus();
            }

            hitData = (control, (Vector2i)rel);
            return true;
        }

        /// <inheritdoc/>
        public void HandleCanFocusUp()
        {
            // NOTE: Since Elona is a keyboard-focused game, it doesn't make sense
            // to detatch focus if the mouse is moved outside a control.
            // ControlFocused = null;
        }

        public void KeyBindDown(BoundKeyEventArgs args)
        {
            var control = ControlFocused ?? KeyboardFocused ?? MouseGetControl(args.PointerLocation);

            if (control == null)
            {
                return;
            }

            var guiArgs = new GUIBoundKeyEventArgs(args.Function, args.State, args.PointerLocation, args.CanFocus,
                args.PointerLocation.Position - control.PixelPosition);

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
                args.PointerLocation.Position - control.PixelPosition);

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
            if (newHovered != CurrentlyHovered && newHovered != null)
            {
                Logger.DebugS("ui.input", $"HOVER: {CurrentlyHovered} -> {newHovered}");
                CurrentlyHovered?.MouseExited();
                CurrentlyHovered = newHovered;
                CurrentlyHovered?.MouseEntered();
            }

            var target = ControlFocused ?? newHovered;
            if (target != null)
            {
                var pos = mouseMoveEventArgs.Position.Position;
                var guiArgs = new GUIMouseMoveEventArgs(mouseMoveEventArgs.Relative / target.UIScale,
                    target,
                    pos / target.UIScale, mouseMoveEventArgs.Position,
                    pos / target.UIScale - target.Position,
                    pos - target.PixelPosition);

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
                pos / control.UIScale, args.Position,
                pos / control.UIScale - control.Position, pos - control.PixelPosition);

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

        private void _doMouseGuiInput<T>(UiElement? control, T guiEvent, Action<UiElement, T> action,
            bool ignoreStop = false)
            where T : GUIMouseEventArgs
        {
            var lastHaltCounter = _inputManager.HaltCounter;

            while (control != null)
            {
                guiEvent.RelativePixelPosition = guiEvent.GlobalPixelPosition.Position - control.PixelPosition;

                guiEvent.SourceControl = control;
                if (control.EventFilter != UIEventFilterMode.Ignore)
                {
                    action(control, guiEvent);

                    var wasHaltInputCalled = lastHaltCounter != _inputManager.HaltCounter;
                    if (guiEvent.Handled || (!ignoreStop && control.EventFilter == UIEventFilterMode.Stop) || wasHaltInputCalled)
                    {
                        break;
                    }
                }

                control = control.Parent;
            }
        }

        private void _doGuiInput<T>(UiElement? control, T guiEvent, Action<UiElement, T> action,
            bool ignoreStop = false)
            where T : GUIBoundKeyEventArgs
        {
            var lastHaltCounter = _inputManager.HaltCounter;

            while (control != null)
            {
                guiEvent.RelativePixelPosition = guiEvent.PointerLocation.Position - control.PixelPosition;

                if (control.EventFilter != UIEventFilterMode.Ignore && !EventFiltered(control, guiEvent))
                {
                    action(control, guiEvent);

                    var wasHaltInputCalled = lastHaltCounter != _inputManager.HaltCounter;
                    if (guiEvent.Handled || (!ignoreStop && control.EventFilter == UIEventFilterMode.Stop) || wasHaltInputCalled)
                    {
                        break;
                    }
                }

                control = control.Parent;
            }
        }

        private bool EventFiltered<T>(UiElement control, T guiEvent) 
            where T : GUIBoundKeyEventArgs
        {
            foreach (var filter in control.BoundKeyEventFilters)
            {
                if (!filter.FilterEvent(control, guiEvent))
                    return true;
            }

            return false;
        }
    }
}
