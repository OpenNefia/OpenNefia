using Love;
using OpenNefia.Core.Data.Types;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.UI.Element
{
    public abstract class BaseInputUiElement : BaseUiElement, IUiInputElement
    {
        protected IInputHandler InputHandler;
        public KeybindWrapper Keybinds { get; }
        public MouseBindWrapper MouseButtons { get; }

        // The empty setters are to support += syntax.

        public TextInputWrapper TextInput { get; }

        private MouseMovedWrapper _MouseMoved;
        public MouseMovedWrapper MouseMoved
        {
            get => _MouseMoved;
            set { }
        }

        private InputForwardsWrapper _Forwards;
        public InputForwardsWrapper Forwards { 
            get => _Forwards; 
            set {}
        }

        public bool TextInputEnabled {
            get => this.InputHandler.TextInputEnabled;
            set => this.InputHandler.TextInputEnabled = value;
        }

        public BaseInputUiElement()
        {
            this.InputHandler = new InputHandler();
            this.Keybinds = new KeybindWrapper(this.InputHandler);
            this.MouseButtons = new MouseBindWrapper(this.InputHandler);
            this._MouseMoved = new MouseMovedWrapper(this.InputHandler);
            this.TextInput = new TextInputWrapper(this.InputHandler);
            this._Forwards = new InputForwardsWrapper(this.InputHandler);
        }

        public void ReceiveKeyPressed(KeyConstant key, bool isRepeat) => InputHandler.ReceiveKeyPressed(key, isRepeat);
        public void ReceiveKeyReleased(KeyConstant key) => InputHandler.ReceiveKeyReleased(key);
        public void ReceiveTextInput(string text) => InputHandler.ReceiveTextInput(text);
        public void ReceiveMouseMoved(float x, float y, float dx, float dy, bool isTouch) => InputHandler.ReceiveMouseMoved(x, y, dx, dy, isTouch);
        public void ReceiveMousePressed(float x, float y, int button, bool isTouch) => InputHandler.ReceiveMousePressed(x, y, button, isTouch);
        public void ReceiveMouseReleased(float x, float y, int button, bool isTouch) => InputHandler.ReceiveMouseReleased(x, y, button, isTouch);

        public void BindKey(IKeybind keybind, Action<KeyInputEvent> func, bool trackReleased = false) => InputHandler.BindKey(keybind, func, trackReleased);
        public void UnbindKey(IKeybind keybind) => InputHandler.UnbindKey(keybind);
        public void HaltInput() => InputHandler.HaltInput();
        public bool IsModifierHeld(Keys modifier) => InputHandler.IsModifierHeld(modifier);
        public void UpdateKeyRepeats(float dt) => InputHandler.UpdateKeyRepeats(dt);
        public void RunKeyActions(float dt) => InputHandler.RunKeyActions(dt);
        public bool RunKeyAction(Keys key, KeyPressState state) => InputHandler.RunKeyAction(key, state);
        public void ReleaseKey(Keys key) => InputHandler.ReleaseKey(key);

        public void BindMouseButton(MouseButtons button, Action<MouseButtonEvent> handler, bool trackReleased = false) => InputHandler.BindMouseButton(button, handler, trackReleased);
        public void UnbindMouseButton(MouseButtons button) => InputHandler.UnbindMouseButton(button);
        public void BindMouseMoved(Action<MouseMovedEvent> handler) => InputHandler.BindMouseMoved(handler);
        public void UnbindMouseMoved() => InputHandler.UnbindMouseMoved();
        public bool RunMouseAction(MouseButtonPress press) => InputHandler.RunMouseAction(press);
        public bool RunMouseMovedAction(int x, int y, int dx, int dy) => InputHandler.RunMouseMovedAction(x, y, dx, dy);
        public void ReleaseMouseButton(MouseButtonPress press) => InputHandler.ReleaseMouseButton(press);

        public bool RunTextInputAction(string text) => InputHandler.RunTextInputAction(text);
        public void BindTextInput(Action<TextInputEvent> evt) => InputHandler.BindTextInput(evt);
        public void UnbindTextInput() => InputHandler.UnbindTextInput();

        public void ForwardTo(IInputHandler keys, int? priority = null) => InputHandler.ForwardTo(keys, priority);
        public void UnforwardTo(IInputHandler keys) => InputHandler.UnforwardTo(keys);
        public void ClearAllForwards() => InputHandler.ClearAllForwards();

        public virtual List<UiKeyHint> MakeKeyHints() => new List<UiKeyHint>();
    }
}
