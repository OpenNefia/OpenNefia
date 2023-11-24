using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;

namespace OpenNefia.Content.DebugView
{
    public partial class NewMapDialog : DefaultWindow
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfer = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public NewMapDialog()
        {
            EntitySystem.InjectDependencies(this);
            OpenNefiaXamlLoader.Load(this);

            OKButton.OnPressed += OnOKButtonPressed;
            CancelButton.OnPressed += OnCancelButtonPressed;
        }

        private void OnOKButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            // TODO number control...
            var widthStr = WidthLineEdit.Text;
            var heightStr = HeightLineEdit.Text;

            if (!int.TryParse(widthStr, out var width) || !int.TryParse(heightStr, out var height) || width < 1 || height < 1)
                return;

            var map = _mapManager.CreateMap(width, height);
            foreach (var tile in map.AllTiles)
                map.SetTile(tile.Position, Protos.Tile.Grass);

            var player = _gameSession.Player;
            var spatial = _entityManager.GetComponent<SpatialComponent>(player);
            _mapTransfer.DoMapTransfer(spatial, map, new CenterMapLocation(), MapLoadType.Full);

            Close();
        }

        private void OnCancelButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            Close();
        }
    }
}
