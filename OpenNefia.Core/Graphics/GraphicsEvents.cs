using Love;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Graphics
{
    public class WindowFocusedEventArgs : EventArgs
    {
        public WindowFocusedEventArgs(bool focused)
        {
            Focused = focused;
        }

        public bool Focused { get; }
    }

    public class WindowResizedEventArgs : EventArgs
    {
        public WindowResizedEventArgs(Vector2i newSize)
        {
            NewSize = newSize;
        }

        public Vector2i NewSize { get; }
    }

    public class QuitEventArgs : EventArgs
    {
    }
}
