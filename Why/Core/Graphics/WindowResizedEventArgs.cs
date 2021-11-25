using Why.Core.Maths;

namespace Why.Core.Graphics
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