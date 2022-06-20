using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;

namespace OpenNefia.Content.DebugView
{
    public partial class AllInOneWindow : DefaultWindow
    {
        public AllInOneWindow()
        {
            OpenNefiaXamlLoader.Load(this);
        }
    }
}
