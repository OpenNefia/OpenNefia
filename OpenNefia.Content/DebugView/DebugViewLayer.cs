using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Core.UI.Wisp.WispControl;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.ViewVariables;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.DebugView
{
    public interface IDebugViewLayer : IUiLayerWithResult<UINone, UINone>, IWispLayer
    {
    }

    public sealed class DebugViewLayer : WispLayerWithResult<UINone, UINone>, IDebugViewLayer
    {
        // The dependency on IFieldLayer is why this lives in content instead of core.
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IViewVariablesManager _viewVariables = default!;

        private bool _initialized = false;

        public DebugViewLayer()
        {
            OnKeyBindDown += HandleKeyBindDown;
            OnKeyBindUp += HandleKeyBindUp;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            var controlTestWindow = new ControlTestMainWindow();
            this.OpenWindowToLeft(controlTestWindow);

            var controlDebugWindow = new ControlDebugWindow();
            this.OpenWindowCentered(controlDebugWindow);

            {
                var win = new DefaultWindow()
                {
                    Title = "Tools",
                };
                var box = new BoxContainer()
                {
                    Orientation = LayoutOrientation.Vertical
                };
                var tilePickerButton = new Button()
                {
                    Text = "Tile Picker",
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 4),
                };
                tilePickerButton.OnPressed += _ => OpenTilePicker();
                box.AddChild(tilePickerButton);
                win.Contents.AddChild(box);
                this.OpenWindowAt(win, controlTestWindow.Rect.TopRight);
            }

            _tilePicker = new TilePickerWindow();
            _tilePicker.OnTileButtonPressed += SetSelectedTile;
            _tileBatch = new TileAtlasBatch(AtlasNames.Tile);

            _initialized = true;
        }

        private TilePrototype? _selectedTile;
        private TilePickerWindow _tilePicker = default!;
        private bool CanPlaceTile => _selectedTile != null && _tilePicker.IsOpen;
        private TileAtlasBatch _tileBatch = default!;
        private bool _placingTile = false;

        private void OpenTilePicker()
        {
            if (_tilePicker.IsOpen)
                _tilePicker.GrabFocus();
            else
                this.OpenWindowCentered(_tilePicker);
        }

        private void SetSelectedTile(TilePickerWindow.TileButtonPressedEventArgs args)
        {
            _selectedTile = args.TilePrototype;
            _tileBatch.Clear();
            // TODO separate scaling for tile viewport/UI
            _tileBatch.Add(1, _selectedTile.Image.AtlasIndex, 0, 0);
            _tileBatch.Flush();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Initialize();
        }

        public override void Draw()
        {
            if (CanPlaceTile)
            {
                if (_mapManager.ActiveMap != null)
                {
                    var tilePos = _field.Camera.VisibleScreenToTile(UserInterfaceManager.MousePositionScaled.Position);
                    var screenPos = _field.Camera.TileToVisibleScreen(_mapManager.ActiveMap.AtPos(tilePos));
                    // TODO separate scaling for tile viewport/UI
                    _tileBatch.Draw(1, screenPos.X, screenPos.Y, color: new(1, 1, 1, 0.25f));

                }
            }

            base.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (!CanPlaceTile)
                _placingTile = false;

            if (_placingTile && _mapManager.ActiveMap != null)
            {
                var pos = _field.Camera.VisibleScreenToTile(UserInterfaceManager.MousePositionScaled.Position);
                _mapManager.ActiveMap.SetTile(pos, _selectedTile!.GetStrongID());
                _mapManager.ActiveMap.MemorizeTile(pos);
                _field.RefreshScreen();
            }
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
            else if (args.Function == EngineKeyFunctions.UIClick)
            {
                _placingTile = true;
            }
            else if (args.Function == EngineKeyFunctions.UIRightClick)
            {
                if (_mapManager.ActiveMap != null)
                {
                    var pos = _field.Camera.VisibleScreenToTile(args.PointerLocation.Position);
                    Logger.InfoS("debugview", $"{pos}");
                    OnRightClick(_mapManager.ActiveMap, pos);
                }
            }
        }

        private void HandleKeyBindUp(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UIClick)
            {
                _placingTile = false;
            }
        }

        private void OnRightClick(IMap map, Vector2i pos)
        {
            if (!map.IsInBounds(pos))
                return;

            var coords = map.AtPos(pos);

            var lookup = EntitySystem.Get<IEntityLookup>();

            foreach (var entity in lookup.GetLiveEntitiesAtCoords(coords))
            {
                _viewVariables.OpenVV(entity, this);
            }
        }
    }
}