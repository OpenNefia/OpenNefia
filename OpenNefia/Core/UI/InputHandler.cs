using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.UI
{
    internal class InputHandler : IInputHandler
    {
        private class KeyAction
        {
            private Action<UiKeyInputEventArgs> Callback;
            public bool TrackReleased { get; private set; }

            public KeyAction(Action<UiKeyInputEventArgs> func, bool trackReleased)
            {
                this.Callback = func;
                this.TrackReleased = trackReleased;
            }

            public void Run(UiKeyInputEventArgs evt) => this.Callback(evt);
        }

        private class MouseButtonAction
        {
            private Action<UiMousePressedEventArgs> Callback;
            public bool TrackReleased { get; private set; }

            public MouseButtonAction(Action<UiMousePressedEventArgs> func, bool trackReleased)
            {
                this.Callback = func;
                this.TrackReleased = trackReleased;
            }

            public void Run(UiMousePressedEventArgs evt) => this.Callback(evt);
        }

        private class KeyRepeatDelay
        {
            public bool IsActive = false;
            public int WaitRemain = 0;
            public float Delay = 0.0f;
            public bool IsPressed = false;
            public bool IsRepeating = false;
            public bool IsFast = false;
            public bool FirstPress = true;
            public HashSet<IKeybind> ActiveKeybinds = new HashSet<IKeybind>();

            public void Reset()
            {
                this.IsActive = false;
                this.WaitRemain = 0;
                this.Delay = 0.0f;
                this.IsPressed = false;
                this.IsRepeating = false;
                this.IsFast = false;
                this.FirstPress = true;
                this.ActiveKeybinds = new HashSet<IKeybind>();
            }
        }

        private HashSet<Keys> KeysPressed = new();
        private HashSet<MouseButtons> MousePressed = new();
        private List<MousePressedEventArgs> MousePressedThisFrame = new();
        private List<MousePressedEventArgs> MouseUnpressedThisFrame = new();
        private Dictionary<Keys, KeyRepeatDelay> RepeatDelays = new();
        private HashSet<Keys> UnpressedThisFrame = new();
        private Keys Modifiers;
        private List<IInputHandler> Forwards = new();
        private Dictionary<IKeybind, KeyAction> Actions = new();
        private Action<UiTextInputEventArgs>? TextInputHandler;
        private Dictionary<MouseButtons, MouseButtonAction> MouseButtonActions = new();
        private Action<UiMouseMovedEventArgs>? MouseMovedHandler;
        private KeybindTranslator Keybinds = new();
        private bool Halted;
        private bool StopHalt;
        private TextInputEventArgs? TextInputThisFrame;
        private bool EnterReceivedThisFrame;
        private MouseMovedEventArgs? MouseMovedThisFrame;

        public bool NoShiftDelay { get; set; }
        public int KeyHeldFrames { get; private set; }
        public bool TextInputEnabled { get; set; }

        /// <inheritdoc />
        public void ReceiveKeyPressed(KeyPressedEventArgs args)
        {
            foreach (var forward in this.Forwards)
            {
                forward.ReceiveKeyPressed(args);
            }

            if (this.Halted && args.IsRepeat) {
                return;
            }

            var key = (Keys)args.Key;

            var modifier = InputUtils.GetModifier(args.Key);
            if (modifier.HasValue)
            {
                this.Modifiers |= modifier.Value;

                // Treat LShift and RShift as Shift, etc.
                key = modifier.Value;
            }

            this.KeysPressed.Add(key);

            if (!this.RepeatDelays.ContainsKey(key))
                this.RepeatDelays[key] = new KeyRepeatDelay();
            this.RepeatDelays[key].IsActive = true;

            // When IME input is sent, LÖVE first sends the keypress event of
            // the user pressing "return" to confirm the IME submission, then
            // it sends a text event with the text the IME entered in the same
            // frame. Therefore, text shouldn't be submitted if a "return"
            // keypress event is not the very last event received in this
            // frame.
            if (this.TextInputThisFrame == null && args.Key == Love.KeyConstant.Enter)
            {
                this.EnterReceivedThisFrame = true;
            }
        }

        /// <inheritdoc />
        public void ReceiveKeyReleased(KeyPressedEventArgs args)
        {
            foreach (var forward in this.Forwards)
            {
                forward.ReceiveKeyReleased(args);
            }

            var key = (Keys)args.Key;

            var modifier = InputUtils.GetModifier(args.Key);
            if (modifier.HasValue)
            {
                this.Modifiers &= ~modifier.Value;

                // Treat LShift and RShift as Shift, etc.
                key = modifier.Value;
            }

            this.UnpressedThisFrame.Add(key);
        }

        /// <inheritdoc />
        public void ReceiveTextInput(TextInputEventArgs args)
        {
            foreach (var forward in this.Forwards)
            {
                forward.ReceiveTextInput(args);
            }

            if (!this.TextInputEnabled)
                return;

            // Prevent Enter from being received if it is followed by an IME
            // text input event in the same frame.
            if (this.EnterReceivedThisFrame)
            {
                this.KeysPressed.Remove(Keys.Enter);
                this.RepeatDelays[Keys.Enter].Reset();
                this.EnterReceivedThisFrame = false;
            }

            this.TextInputThisFrame = args;
        }

        public void ReceiveMouseMoved(MouseMovedEventArgs args)
        {
            this.MouseMovedThisFrame = args;
        }

        public void ReceiveMousePressed(MousePressedEventArgs args)
        {
            this.MousePressedThisFrame.Add(args);
        }

        public void ReceiveMouseReleased(MousePressedEventArgs args)
        {
            this.MouseUnpressedThisFrame.Add(args);
        }

        public void HaltInput()
        {
            foreach (var key in this.KeysPressed)
            {
                this.ReleaseKey(key);
            }
            foreach (var button in this.MousePressed)
            {
                this.ReleaseMouseButton(new MousePressedEventArgs(new Vector2i((int)Love.Mouse.GetX(), (int)Love.Mouse.GetY()), button, false, false));
            }

            this.RepeatDelays.Clear();
            this.Modifiers = Keys.None;
            this.KeysPressed.Clear();
            this.UnpressedThisFrame.Clear();
            this.Halted = true;
            this.StopHalt = false;
            this.KeyHeldFrames = 0;
            this.TextInputThisFrame = null;
            this.EnterReceivedThisFrame = false;
            this.MouseMovedThisFrame = null;

            foreach (var forward in this.Forwards)
            {
                forward.HaltInput();
            }
        }

        public void UpdateKeyRepeats(float dt)
        {
            foreach (var key in this.KeysPressed)
            {
                var keyRepeat = this.RepeatDelays[key];
                var keybind = this.Keybinds.KeyToKeybind(key | this.Modifiers);
                var isShiftDelayed = keybind != null && keybind.IsShiftDelayed;

                if (keyRepeat.FirstPress)
                {
                    keyRepeat.FirstPress = false;

                    if (isShiftDelayed)
                    {
                        if (this.NoShiftDelay)
                        {
                            keyRepeat.WaitRemain = 0;
                            keyRepeat.Delay = 40;
                        }
                        else
                        {
                            keyRepeat.WaitRemain = 3;
                            keyRepeat.Delay = 200;
                        }
                    }
                    else
                    {
                        keyRepeat.WaitRemain = 0;
                        keyRepeat.Delay = 600;
                    }
                    keyRepeat.IsPressed = true;
                }

                keyRepeat.Delay -= dt * 1000f;
                if (keyRepeat.Delay <= 0)
                {
                    keyRepeat.IsPressed = true;
                }

                if (isShiftDelayed && (this.Modifiers & Keys.Shift) == Keys.Shift)
                {
                    keyRepeat.Delay = 10;
                }
            }

            foreach (var forward in this.Forwards)
            {
                forward.UpdateKeyRepeats(dt);
            }
        }

        public void ForwardTo(IInputHandler keys, int? priority = null)
        {
            if (this == keys)
                throw new ArgumentException("Cannot forward key handler to itself");

            this.Forwards.Add(keys);
        }

        public void UnforwardTo(IInputHandler keys)
        {
            this.Forwards.Remove(keys);
        }
        
        public void ClearAllForwards()
        {
            this.Forwards.Clear();
        }

        private bool AddKeyDelay(Keys keyWithoutModifiers, bool isShiftDelayed)
        {
            if (!this.RepeatDelays.ContainsKey(keyWithoutModifiers))
                this.RepeatDelays[keyWithoutModifiers] = new KeyRepeatDelay();
            var keyRepeat = this.RepeatDelays[keyWithoutModifiers]!;

            keyRepeat!.WaitRemain--;
            if (keyRepeat.WaitRemain <= 0)
            {
                if (isShiftDelayed)
                {
                    if (this.NoShiftDelay)
                    {
                        keyRepeat.Delay = 100;
                    }
                    else
                    {
                        keyRepeat.Delay = 20;
                    }
                }
                if (keyRepeat.IsFast)
                {
                    keyRepeat.IsRepeating = true;
                }
                keyRepeat.IsFast = true;
            }
            else if (keyRepeat.IsFast)
            {
                if (isShiftDelayed)
                {
                    // TODO
                    if (this.NoShiftDelay)
                    {
                        keyRepeat.Delay = 100;
                    }
                    else
                    {
                        keyRepeat.Delay = 20;
                    }
                }
                else
                {
                    keyRepeat.Delay = 10;
                }
            }
            else
            {
                keyRepeat.Delay = 200;
            }
            keyRepeat.IsPressed = false;

            return keyRepeat.IsRepeating;
        }

        public bool RunKeyAction(Keys keyAndModifiers, KeyPressState state)
        {
            var keyWithoutModifiers = keyAndModifiers & (~Keys.AllModifiers);

            if (this.RepeatDelays.TryGetValue(keyWithoutModifiers, out KeyRepeatDelay? repeatDelay))
            {
                if (repeatDelay.IsPressed)
                {
                    var keybind = this.Keybinds.KeyToKeybind(keyAndModifiers);
                    var isShiftDelayed = keybind != null && keybind.IsShiftDelayed;

                    var isRepeating = this.AddKeyDelay(keyWithoutModifiers, isShiftDelayed);

                    if (keybind != null)
                    {
                        if (this.Actions.TryGetValue(keybind, out KeyAction? action))
                        {
                            if (state != KeyPressState.Released || (state == KeyPressState.Released && action.TrackReleased))
                            {
                                if (isRepeating && state == KeyPressState.Pressed)
                                    state = KeyPressState.Repeated;

                                var evt = new UiKeyInputEventArgs(keybind, state);
                                action.Run(evt);
                                if (!evt.Passed)
                                {
                                    repeatDelay.ActiveKeybinds.Add(keybind);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var forward in this.Forwards)
            {
                if (forward.RunKeyAction(keyAndModifiers, state))
                {
                    return true;
                }
            }

            return false;
        }

        public bool RunMouseAction(MousePressedEventArgs press)
        {
            if (this.MouseButtonActions.TryGetValue(press.Button, out MouseButtonAction? action))
            {
                var evt = new UiMousePressedEventArgs(press.Button, KeyPressState.Pressed, (Vector2i)press.Pos);
                action.Run(evt);
                if (!evt.Passed)
                {
                    return true;
                }
            }

            foreach (var forward in this.Forwards)
            {
                if (forward.RunMouseAction(press))
                {
                    return true;
                }
            }

            return false;
        }

        public bool RunTextInputAction(TextInputEventArgs args)
        {
            if (this.TextInputHandler != null)
            {
                var evt = new UiTextInputEventArgs(args.Text);
                this.TextInputHandler(evt);
                if (!evt.Passed)
                {
                    return true;
                }
            }

            foreach (var forward in this.Forwards)
            {
                if (forward.RunTextInputAction(args.Text))
                {
                    return true;
                }
            }

            return false;
        }

        public bool RunMouseMovedAction(MouseMovedEventArgs args)
        {
            if (this.MouseMovedHandler != null)
            {
                var evt = new UiMouseMovedEventArgs((Vector2i)args.Pos, (Vector2i)args.DPos);
                this.MouseMovedHandler(evt);
                if (!evt.Passed)
                {
                    return true;
                }
            }

            foreach (var forward in this.Forwards)
            {
                if (forward.RunMouseMovedAction(args))
                {
                    return true;
                }
            }

            return false;
        }

        public void BindKey(IKeybind keybind, Action<UiKeyInputEventArgs> handler, bool trackReleased = false)
        {
            this.Actions[keybind] = new KeyAction(handler, trackReleased);
            this.Keybinds.Enable(keybind);
        }

        public void UnbindKey(IKeybind keybind)
        {
            this.Actions.Remove(keybind);
            this.Keybinds.Disable(keybind);
        }

        public void BindMouseButton(MouseButtons button, Action<UiMousePressedEventArgs> handler, bool trackReleased = false)
        {
            this.MouseButtonActions[button] = new MouseButtonAction(handler, trackReleased);
        }

        public void UnbindMouseButton(MouseButtons button)
        {
            this.MouseButtonActions.Remove(button);
        }

        public void BindMouseMoved(Action<UiMouseMovedEventArgs> handler)
        {
            this.MouseMovedHandler = handler;
        }

        public void UnbindMouseMoved()
        {
            this.MouseMovedHandler = null;
        }

        public void BindTextInput(Action<UiTextInputEventArgs> handler)
        {
            this.TextInputHandler = handler;
        }

        public void UnbindTextInput()
        {
            this.TextInputHandler = null;
        }

        public bool IsModifierHeld(Keys modifier)
        {
            return (this.Modifiers & modifier) == modifier;
        }

        public void ReleaseKey(Keys key, bool runEvents = true)
        {
            this.KeysPressed.Remove(key);

            if (this.RepeatDelays.TryGetValue(key & (~Keys.AllModifiers), out var repeatDelay))
            {
                if (runEvents)
                {
                    foreach (var activeKeybind in repeatDelay.ActiveKeybinds)
                    {
                        if (this.Actions.TryGetValue(activeKeybind, out KeyAction? action))
                        {
                            if (action.TrackReleased)
                            {
                                var evt = new UiKeyInputEventArgs(activeKeybind, KeyPressState.Released);
                                action.Run(evt);
                                runEvents = evt.Passed;
                            }
                        }
                    }
                }

                repeatDelay.Reset();
            }

            foreach (var forward in this.Forwards)
            {
                forward.ReleaseKey(key, runEvents);
            }
        }

        public void ReleaseMouseButton(MousePressedEventArgs press, bool runEvents = true)
        {
            this.MousePressed.Remove(press.Button);

            if (runEvents && this.MouseButtonActions.TryGetValue(press.Button, out var action))
            {
                if (action.TrackReleased)
                {
                    var evt = new UiMousePressedEventArgs(press.Button, KeyPressState.Released, (Vector2i)Love.Mouse.GetPosition());
                    action.Run(evt);
                    runEvents = evt.Passed;
                }
            }

            foreach (var forward in this.Forwards)
            {
                forward.ReleaseMouseButton(press, runEvents);
            }
        }

        public void RunKeyActions(float dt)
        {
            foreach (var key in this.UnpressedThisFrame)
            {
                this.ReleaseKey(key);
            }
            foreach (var press in this.MouseUnpressedThisFrame)
            {
                this.ReleaseMouseButton(press);
            }

            var ran = false;

            foreach (var (key, repeatDelay) in this.RepeatDelays)
            {
                if (repeatDelay.IsActive)
                {
                    ran = this.RunKeyAction(key | this.Modifiers, KeyPressState.Pressed);
                    if (ran)
                    {
                        // Only run the first key action.
                        break;
                    }
                }
            }

            foreach (var press in this.MousePressedThisFrame)
            {
                this.RunMouseAction(press);
            }

            if (this.MouseMovedThisFrame != null)
            {
                this.RunMouseMovedAction(this.MouseMovedThisFrame.Value);

                this.MouseMovedThisFrame = null;
            }

            if (this.TextInputThisFrame != null)
            {
                if (this.TextInputEnabled)
                {
                    this.RunTextInputAction(this.TextInputThisFrame.Value);
                }

                this.TextInputThisFrame = null;
            }

            this.UnpressedThisFrame.Clear();
            this.MousePressedThisFrame.Clear();
            this.MouseUnpressedThisFrame.Clear();
            this.EnterReceivedThisFrame = false;

            this.UpdateKeyRepeats(dt);

            this.Halted = this.Halted && !this.StopHalt;

            if (this.KeysPressed.Count > 0)
            {
                if (ran)
                {
                    this.KeyHeldFrames++;
                }
            }
            else
            {
                this.KeyHeldFrames = 0;
            }
        }
    }
}
