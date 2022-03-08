using OpenNefia.Core.UI.Wisp.CustomControls;

namespace OpenNefia.Core.UI.Wisp
{
    public static class IWispLayerExtensions
    {
        public static void OpenWindow(this IWispLayer layer, BaseWindow window)
        {
            window.Open(layer);
        }

        public static void OpenWindowCentered(this IWispLayer layer, BaseWindow window)
        {
            window.OpenCentered(layer);
        }

        public static void OpenWindowToLeft(this IWispLayer layer, BaseWindow window)
        {
            window.OpenToLeft(layer);
        }
    }
}
