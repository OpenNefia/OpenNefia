namespace Why.Core.Graphics
{
    public readonly struct WindowFocusedEventArgs
    {
        public WindowFocusedEventArgs(bool focused)
        {
            Focused = focused;
        }

        public bool Focused { get; }
    }
}
