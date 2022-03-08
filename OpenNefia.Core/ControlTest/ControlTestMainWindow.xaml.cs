using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;

namespace OpenNefia.Core.ControlTest
{
    public partial class ControlTestMainWindow : DefaultWindow
    {
        public ControlTestMainWindow()
        {
            OpenNefiaXamlLoader.Load(this);

            AllInOneButton.OnPressed += _ => ShowWindow();
            ItemListButton.OnPressed += _ => ShowWindow();
        }

        private void ShowWindow()
        {
            var win = new DefaultWindow();
            WispRootLayer!.OpenWindowCentered(win);
        }
    }
}
