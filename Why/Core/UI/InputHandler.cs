using OpenNefia.Core.Data.Types;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.UI
{
    internal class InputHandler : IInputHandler
    {
        private class KeyAction
        {
            private Action<KeyInputEvent> Callback;
            public bool TrackReleased { get; private set; }

            public KeyAction(Action<KeyInputEvent> func, bool trackReleased)
            {
                this.Callback = func;
                this.TrackReleased = trackReleased;
            }

            public void Run(KeyInputEvent evt) => this.Callback(evt);
        }

        private class MouseButtonAction
        {
            private Action<MouseButtonEvent> Callback;
            public bool TrackReleased { get; private set; }

            public MouseButtonAction(Action<MouseButtonEvent> func, bool trackReleased)
            {
                this.Callback = func;
                this.TrackReleased = trackReleased;
            }

            public void Run(MouseButtonEvent evt) => this.Callback(evt);
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

        private HashSet<Keys> KeysPressed;
        private HashSet<MouseButtons> MousePressed;
        private List<MouseButtonPress> MousePressedThisFrame;
        private List<MouseButtonPress> MouseUnpressedThisFrame;
        private Dictionary<Keys, KeyRepeatDelay> RepeatDelays;
        private HashSet<Keys> UnpressedThisFrame;
        private Keys Modifiers;
        private List<IInputHandler> Forwards;
        private Dictionary<IKeybind, KeyAction> Actions;
        private Action<TextInputEvent>? TextInputHandler;
        private Dictionary<MouseButtons, MouseButtonAction> MouseButtonActions;
        private Action<MouseMovedEvent>? MouseMovedHandler;
        private KeybindTranslator Keybinds;
        private bool Halted;
        private bool StopHalt;
        private string? TextInputThisFrame;
        private bool EnterReceivedThisFrame;
        private bool WasMouseMovedThisFrame;
        private int MouseMovedXThisFrame;
        private int MouseMovedYThisFrame;
        private int MouseMovedDXThisFrame;
        private int MouseMovedDYThisFrame;

        public bool NoShiftDelay { get; set; }
        public int KeyHeldFrames { get; private set; }
        public bool TextInputEnabled { get; set; }

        public InputHandler()
        {
            this.KeysPressed = new HashSet<Keys>();
            this.MousePressedThisFrame = new List<MouseButtonPress>();
            this.MouseUnpressedThisFrame = new List<MouseButtonPress>();
            this.MousePressed = new HashSet<MouseButtons>();
            this.RepeatDelays = new Dictionary<Keys, KeyRepeatDelay>();  
            this.UnpressedThisFrame = new HashSet<Keys>();
            this.Modifiers = Keys.None;
            this.Forwards = new List<IInputHandler>();
            this.Actions = new Dictionary<IKeybind, KeyAction>();
            this.Keybinds = new KeybindTranslator();
            this.Halted = false;
            this.StopHalt = false;
            this.NoShiftDelay = false;
            this.KeyHeldFrames = 0;
            this.TextInputEnabled = false;
            this.TextInputThisFrame = null;
            this.EnterReceivedThisFrame = false;
            this.TextInputHandler = null;
            this.MouseButtonActions = new Dictionary<MouseButtons, MouseButtonAction>();
            this.MouseMovedHandler = null;
            this.WasMouseMovedThisFrame = false;
            this.MouseMovedXThisFrame = 0;
            this.MouseMovedYThisFrame = 0;
            this.MouseMovedDXThisFrame = 0;
            this.MouseMovedDYThisFrame = 0;
        }

        /// <inheritdoc />
        public void ReceiveKeyPressed(Love.KeyConstant loveKey, bool isRepeat)
        {
            foreach (var forward in this.Forwards)
            {
                forward.ReceiveKeyPressed(loveKey, isRepeat);
            }

            if (this.Halted && isRepeat) {
                return;
            }

            var key = (Keys)loveKey;

            var modifier = InputUtils.GetModifier(loveKey);
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
            if (this.TextInputThisFrame == null && loveKey == Love.KeyConstant.Enter)
            {
                this.EnterReceivedThisFrame = true;
            }
        }

        /// <inheritdoc />
        public void ReceiveKeyReleased(Love.KeyConstant loveKey)
        {
            foreach (var forward in this.Forwards)
            {
                forward.ReceiveKeyReleased(loveKey);
            }

            var key = (Keys)loveKey;

            var modifier = InputUtils.GetModifier(loveKey);
            if (modifier.HasValue)
            {
                this.Modifiers &= ~modifier.Value;

                // Treat LShift and RShift as Shift, etc.
                key = modifier.Value;
            }

            this.UnpressedThisFrame.Add(key);
        }

        /// <inheritdoc />
        public void ReceiveTextInput(string text)
        {
            foreach (var forward in this.Forwards)
            {
                forward.ReceiveTextInput(text);
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

            this.TextInputThisFrame = text;
        }

        public void ReceiveMouseMoved(float x, float y, float dx, float dy, bool isTouch)
        {
            this.WasMouseMovedThisFrame = true;
            this.MouseMovedXThisFrame = (int)x;
            this.MouseMovedYThisFrame = (int)y;
            this.MouseMovedDXThisFrame = (int)dx;
            this.MouseMovedDYThisFrame = (int)dy;
        }

        public void ReceiveMousePressed(float x, float y, int button, bool isTouch)
        {
            this.MousePressedThisFrame.Add(new MouseButtonPress((MouseButtons)button, (int)x, (int)y));
        }

        public void ReceiveMouseReleased(float x, float y, int button, bool isTouch)
        {
            this.MouseUnpressedThisFrame.Add(new MouseButtonPress((MouseButtons)button, (int)x, (int)y));
        }

        public void HaltInput()
        {
            foreach (var key in this.KeysPressed)
            {
                this.ReleaseKey(key);
            }
            foreach (var button in this.MousePressed)
            {
                this.ReleaseMouseButton(new MouseButtonPress(button, (int)Love.Mouse.GetX(), (int)Love.Mouse.GetY()));
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
            this.WasMouseMovedThisFrame = false;
            this.MouseMovedXThisFrame = 0;
            this.MouseMovedYThisFrame = 0;
            this.MouseMovedDXThisFrame = 0;
            this.MouseMovedDYThisFrame = 0;

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

                                var evt = new KeyInputEvent(state);
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

        public bool RunMouseAction(MouseButtonPress press)
        {
            if (this.MouseButtonActions.TryGetValue(press.Button, out MouseButtonAction? action))
            {
                var evt = new MouseButtonEvent(KeyPressState.Pressed, press.Button, press.X, press.Y);
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

        public bool RunTextInputAction(string text)
        {
            if (this.TextInputHandler != null)
            {
                var evt = new TextInputEvent(text);
                this.TextInputHandler(evt);
                if (!evt.Passed)
                {
                    return true;
                }
            }

            foreach (var forward in this.Forwards)
            {
                if (forward.RunTextInputAction(text))
                {
                    return true;
                }
            }

            return false;
        }

        public bool RunMouseMovedAction(int x, int y, int dx, int dy)
        {
            if (this.MouseMovedHandler != null)
            {
                var evt = new MouseMovedEvent(x, y, dx, dy);
                this.MouseMovedHandler(evt);
                if (!evt.Passed)
                {
                    return true;
                }
            }

            foreach (var forward in this.Forwards)
            {
                if (forward.RunMouseMovedAction(x, y, dx, dy))
                {
                    return true;
                }
            }

            return false;
        }

        public void BindKey(IKeybind keybind, Action<KeyInputEvent> handler, bool trackReleased = false)
        {
            this.Actions[keybind] = new KeyAction(handler, trackReleased);
            this.Keybinds.Enable(keybind);
        }

        public void UnbindKey(IKeybind keybind)
        {
            this.Actions.Remove(keybind);
            this.Keybinds.Disable(keybind);
        }

        public void BindMouseButton(MouseButtons button, Action<MouseButtonEvent> handler, bool trackReleased = false)
        {
            this.MouseButtonActions[button] = new MouseButtonAction(handler, trackReleased);
        }

        public void UnbindMouseButton(MouseButtons button)
        {
            this.MouseButtonActions.Remove(button);
        }

        public void BindMouseMoved(Action<MouseMovedEvent> handler)
        {
            this.MouseMovedHandler = handler;
        }

        public void UnbindMouseMoved()
        {
            this.MouseMovedHandler = null;
        }

        public void BindTextInput(Action<TextInputEvent> handler)
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

        public void ReleaseKey(Keys key)
        {
            this.KeysPressed.Remove(key);

            if (this.RepeatDelays.TryGetValue(key & (~Keys.AllModifiers), out var repeatDelay))
            {
                foreach (var activeKeybind in repeatDelay.ActiveKeybinds)
                {
                    if (this.Actions.TryGetValue(activeKeybind, out KeyAction? action))
                    {
                        if (action.TrackReleased)
                        {
                            var evt = new KeyInputEvent(KeyPressState.Released);
                            action.Run(evt);
                        }
                    }
                }

                repeatDelay.Reset();
            }

            foreach (var forward in this.Forwards)
            {
                forward.ReleaseKey(key);
            }
        }

        public void ReleaseMouseButton(MouseButtonPress press)
        {
            this.MousePressed.Remove(press.Button);

            if (this.MouseButtonActions.TryGetValue(press.Button, out var action))
            {
                if (action.TrackReleased)
                {
                    var evt = new MouseButtonEvent(KeyPressState.Released, press.Button, press.X, press.Y);
                    action.Run(evt);
                }
            }

            foreach (var forward in this.Forwards)
            {
                forward.ReleaseMouseButton(press);
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

            if (this.WasMouseMovedThisFrame)
            {
                this.RunMouseMovedAction(this.MouseMovedXThisFrame, this.MouseMovedYThisFrame, this.MouseMovedDXThisFrame, this.MouseMovedDYThisFrame);

                this.WasMouseMovedThisFrame = false;
            }

            if (this.TextInputThisFrame != null)
            {
                if (this.TextInputEnabled)
                {
                    this.RunTextInputAction(this.TextInputThisFrame);
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
