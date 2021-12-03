using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    /// <summary>
    /// Provides some convenient syntax for defining key bindings on UI classes that support them.
    /// </summary>
    public class KeybindWrapper
    {
        public class KeybindDelegateWrapper : IKeyBinder
        {
            public IKeybind Keybind { get; }
            public KeybindWrapper Parent { get; }

            public KeybindDelegateWrapper(KeybindWrapper parent, IKeybind keybind)
            {
                this.Parent = parent;
                this.Keybind = keybind;
            }

            public static KeybindDelegateWrapper operator +(KeybindDelegateWrapper delegateWrapper, Action<UiKeyInputEventArgs> evt)
            {
                delegateWrapper.BindKey(delegateWrapper.Keybind, evt);
                return delegateWrapper;
            }

            public void Unbind()
            {
                this.UnbindKey(this.Keybind);
            }

            public void BindKey(Action<UiKeyInputEventArgs> func, bool trackReleased = false)
                => this.BindKey(this.Keybind, func, trackReleased);

            public void BindKey(IKeybind keybind, Action<UiKeyInputEventArgs> func, bool trackReleased = false)
            {
                this.Parent.KeyInput.BindKey(keybind, func, trackReleased);
            }

            public void UnbindKey(IKeybind keybind)
            {
                this.Parent.KeyInput.UnbindKey(keybind);
            }
        }

        public IInputHandler KeyInput { get; }

        private Dictionary<IKeybind, KeybindDelegateWrapper> _Cache;

        public KeybindWrapper(IInputHandler parent)
        {
            this.KeyInput = parent;
            this._Cache = new Dictionary<IKeybind, KeybindDelegateWrapper>();
        }

        public KeybindDelegateWrapper this[IKeybind index]
        {
            get {
                if (!this._Cache.ContainsKey(index))
                    this._Cache[index] = new KeybindDelegateWrapper(this, index);
                return this._Cache[index];
            }
            set
            {
            }
        }
        
        public KeybindDelegateWrapper this[Keys index]
        {
            get => this[RawKey.AllKeys[index]!];
            set
            {
            }
        }
    }
}
