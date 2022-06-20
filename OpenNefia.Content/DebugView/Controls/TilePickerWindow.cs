using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Content.DebugView
{
    public partial class TilePickerWindow : DefaultWindow
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public const string StyleClassTileButton = "tileButton";

        public TilePickerWindow()
        {
            IoCManager.InjectDependencies(this);
            OpenNefiaXamlLoader.Load(this);

            Refresh();
        }

        public void Refresh()
        {
            foreach (var tileProto in _protos.EnumeratePrototypes<TilePrototype>())
            {
                var box = new BoxContainer() { Orientation = LayoutOrientation.Vertical };

                var tileView = new TileView() { Tile = tileProto };
                var button = new ContainerButton() {};
                button.AddStyleClass(StyleClassTileButton);
                tileView.AddChild(button);
                box.AddChild(tileView);

                TileGrid.AddChild(box);
            }
        }
    }
}
