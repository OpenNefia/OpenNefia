using System;

namespace OpenNefia.Core.UI
{
    public interface IMouseMovedBinder
    {
        void BindMouseMoved(Action<UiMouseMovedEventArgs> handler);
        void UnbindMouseMoved();
    }
}