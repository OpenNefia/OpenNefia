using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Graphics
{
    public readonly struct WindowResizedEventArgs
    {
        public WindowResizedEventArgs(Vector2i newSize)
        {
            NewSize = newSize;
        }

        public Vector2i NewSize { get; }
    }
}