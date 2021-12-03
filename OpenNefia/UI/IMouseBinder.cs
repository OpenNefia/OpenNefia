using System;

namespace OpenNefia.Core.UI
{
    public interface IMouseBinder
    {
        void BindMouseButton(MouseButton button, Action<UiMousePressedEventArgs> handler, bool trackReleased = false);
        void UnbindMouseButton(MouseButton button);
    }
}