using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Graphics;

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

        public void ReceiveKeyPressed(KeyPressedEventArgs args) => InputHandler.ReceiveKeyPressed(args);
        public void ReceiveKeyReleased(KeyPressedEventArgs args) => InputHandler.ReceiveKeyReleased(args);
        public void ReceiveTextInput(TextInputEventArgs args) => InputHandler.ReceiveTextInput(args);
        public void ReceiveMouseMoved(MouseMovedEventArgs args) => InputHandler.ReceiveMouseMoved(args);
        public void ReceiveMousePressed(MousePressedEventArgs args) => InputHandler.ReceiveMousePressed(args);
        public void ReceiveMouseReleased(MousePressedEventArgs args) => InputHandler.ReceiveMouseReleased(args);

        public void BindKey(IKeybind keybind, Action<UiKeyInputEventArgs> func, bool trackReleased = false) => InputHandler.BindKey(keybind, func, trackReleased);
        public void UnbindKey(IKeybind keybind) => InputHandler.UnbindKey(keybind);
        public void HaltInput() => InputHandler.HaltInput();
        public bool IsModifierHeld(Keys modifier) => InputHandler.IsModifierHeld(modifier);
        public void UpdateKeyRepeats(float dt) => InputHandler.UpdateKeyRepeats(dt);
        public void RunKeyActions(float dt) => InputHandler.RunKeyActions(dt);
        public bool RunKeyAction(Keys key, KeyPressState state) => InputHandler.RunKeyAction(key, state);
        public void ReleaseKey(Keys key, bool runEvents = true) => InputHandler.ReleaseKey(key, runEvents);

        public void BindMouseButton(MouseButtons button, Action<UiMousePressedEventArgs> handler, bool trackReleased = false) => InputHandler.BindMouseButton(button, handler, trackReleased);
        public void UnbindMouseButton(MouseButtons button) => InputHandler.UnbindMouseButton(button);
        public void BindMouseMoved(Action<UiMouseMovedEventArgs> handler) => InputHandler.BindMouseMoved(handler);
        public void UnbindMouseMoved() => InputHandler.UnbindMouseMoved();
        public bool RunMouseAction(MousePressedEventArgs args) => InputHandler.RunMouseAction(args);
        public bool RunMouseMovedAction(MouseMovedEventArgs args) => InputHandler.RunMouseMovedAction(args);
        public void ReleaseMouseButton(MousePressedEventArgs args, bool runEvents = true) => InputHandler.ReleaseMouseButton(args, runEvents);

        public bool RunTextInputAction(TextInputEventArgs args) => InputHandler.RunTextInputAction(args);
        public void BindTextInput(Action<UiTextInputEventArgs> evt) => InputHandler.BindTextInput(evt);
        public void UnbindTextInput() => InputHandler.UnbindTextInput();

        public void ForwardTo(IInputHandler keys, int? priority = null) => InputHandler.ForwardTo(keys, priority);
        public void UnforwardTo(IInputHandler keys) => InputHandler.UnforwardTo(keys);
        public void ClearAllForwards() => InputHandler.ClearAllForwards();

        public virtual List<UiKeyHint> MakeKeyHints() => new List<UiKeyHint>();
    }
}
