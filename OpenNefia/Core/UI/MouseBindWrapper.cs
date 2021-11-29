using OpenNefia.Core.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    /// <summary>
    /// Provides some convenient syntax for defining mouse button bindings on UI classes that support them.
    /// </summary>
    public class MouseBindWrapper
    {
        public class MouseBindDelegateWrapper : IMouseBinder
        {
            public MouseButtons MouseButton { get; }
            public MouseBindWrapper Parent { get; }

            public MouseBindDelegateWrapper(MouseBindWrapper parent, MouseButtons button)
            {
                this.Parent = parent;
                this.MouseButton = button;
            }

            public static MouseBindDelegateWrapper operator +(MouseBindDelegateWrapper delegateWrapper, Action<UiMousePressedEventArgs> evt)
            {
                delegateWrapper.BindMouseButton(delegateWrapper.MouseButton, evt);
                return delegateWrapper;
            }

            public void Unbind()
            {
                this.UnbindMouseButton(this.MouseButton);
            }

            public void BindMouseButton(Action<UiMousePressedEventArgs> func, bool trackReleased = false)
                => this.BindMouseButton(this.MouseButton, func, trackReleased);

            public void BindMouseButton(MouseButtons button, Action<UiMousePressedEventArgs> func, bool trackReleased = false)
            {
                this.Parent.Input.BindMouseButton(button, func, trackReleased);
            }

            public void UnbindMouseButton(MouseButtons button)
            {
                this.Parent.Input.UnbindMouseButton(button);
            }

            public void Bind(Action<UiMousePressedEventArgs> func, bool trackReleased = false) 
                => BindMouseButton(this.MouseButton, func, trackReleased);
        }

        public IInputHandler Input { get; }

        private Dictionary<MouseButtons, MouseBindDelegateWrapper> _Cache;

        public MouseBindWrapper(IInputHandler parent)
        {
            this.Input = parent;
            this._Cache = new Dictionary<MouseButtons, MouseBindDelegateWrapper>();
        }

        public MouseBindDelegateWrapper this[MouseButtons index]
        {
            get
            {
                if (!this._Cache.ContainsKey(index))
                    this._Cache[index] = new MouseBindDelegateWrapper(this, index);
                return this._Cache[index];
            }
            set
            {
            }
        }
    }
}
