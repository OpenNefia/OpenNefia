using System;

namespace OpenNefia.Core.UI
{
    public interface IMouseBinder
    {
        void BindMouseButton(MouseButtons button, Action<UiMousePressedEventArgs> handler, bool trackReleased = false);
        void UnbindMouseButton(MouseButtons button);
    }
}