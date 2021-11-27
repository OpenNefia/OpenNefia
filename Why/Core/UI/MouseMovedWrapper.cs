using OpenNefia.Core.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    /// <summary>
    /// Provides some convenient syntax for defining text input handlers on UI classes that support them.
    /// </summary>
    public class MouseMovedWrapper
    {
        public class MouseMovedDelegateWrapper : IMouseMovedBinder
        {
            public MouseMovedWrapper Parent { get; }

            public MouseMovedDelegateWrapper(MouseMovedWrapper parent)
            {
                this.Parent = parent;
            }

            public static MouseMovedDelegateWrapper operator +(MouseMovedDelegateWrapper forwardsWrapper, Action<MouseMovedEvent> handler)
            {
                forwardsWrapper.BindMouseMoved(handler);
                return forwardsWrapper;
            }

            public void Clear() => this.UnbindMouseMoved();

            public void BindMouseMoved(Action<MouseMovedEvent> handler) => this.Parent.InputHandler.BindMouseMoved(handler);
            public void UnbindMouseMoved() => this.Parent.InputHandler.UnbindMouseMoved();
        }

        public IInputHandler InputHandler { get; }

        // The empty setter is for supporting += syntax.

        private MouseMovedDelegateWrapper _Callback;
        public MouseMovedDelegateWrapper Callback
        {
            get => _Callback;
            set {}
        }

        public MouseMovedWrapper(IInputHandler parent)
        {
            this.InputHandler = parent;
            this._Callback = new MouseMovedDelegateWrapper(this);
        }
    }
}
