using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.IoC;
using OpenNefia.Core.ViewVariables;

namespace OpenNefia.Core.ControlTest
{
    public partial class ControlTestMainWindow : DefaultWindow
    {
        public ControlTestMainWindow()
        {
            OpenNefiaXamlLoader.Load(this);

            AllInOneButton.OnPressed += _ => WispRootLayer!.OpenWindowCentered(new AllInOneWindow());
            ItemListButton.OnPressed += _ => WispRootLayer!.OpenWindowCentered(new DefaultWindow());
            TextureRectButton.OnPressed += _ => WispRootLayer!.OpenWindowCentered(new TextureRectWindow());
            ViewVariablesButton.OnPressed += _ =>
            {
                var vv = IoCManager.Resolve<IViewVariablesManager>();
                vv.OpenVV(this);
            };
        }
    }
}
