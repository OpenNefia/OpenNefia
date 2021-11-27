using System;

namespace OpenNefia.Core.UI
{
    public interface IMouseMovedBinder
    {
        void BindMouseMoved(Action<MouseMovedEvent> handler);
        void UnbindMouseMoved();
    }
}