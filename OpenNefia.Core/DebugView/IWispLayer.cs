using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;

namespace OpenNefia.Core.DebugView
{
    public interface IWispLayer : IUiLayer
    {
        WispRoot WispRoot { get; }

        LayoutContainer WindowRoot { get; }
    }
}