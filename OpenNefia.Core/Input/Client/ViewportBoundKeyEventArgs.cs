using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Input
{
    public sealed class ViewportBoundKeyEventArgs
    {
        public BoundKeyEventArgs KeyEventArgs { get; }
        public UiElement? Viewport { get; }

        public ViewportBoundKeyEventArgs(BoundKeyEventArgs keyEventArgs, UiElement? viewport)
        {
            KeyEventArgs = keyEventArgs;
            Viewport = viewport;
        }
    }
}
