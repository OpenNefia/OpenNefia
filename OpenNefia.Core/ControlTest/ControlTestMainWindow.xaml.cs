using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.IoC;
using OpenNefia.Core.ViewVariables;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Areas;

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
                var session = IoCManager.Resolve<IGameSessionManager>();
                if (session != null)
                    vv.OpenVV(session);
                var map = IoCManager.Resolve<IMapManager>().ActiveMap;
                if (map != null)
                    vv.OpenVV(map);
                var area = IoCManager.Resolve<IAreaManager>().ActiveArea;
                if (area != null)
                    vv.OpenVV(area);
            };
        }
    }
}
