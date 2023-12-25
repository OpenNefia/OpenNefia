using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Controls;
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
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Content.UI;

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
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
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
                    CanClose = false
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

                var entityPickerButton = new Button()
                {
                    Text = "Entity Picker",
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 4),
                };
                entityPickerButton.OnPressed += _ => OpenEntityPicker();
                box.AddChild(entityPickerButton);

                var newMapButton = new Button()
                {
                    Text = "New Map...",
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 4),
                };
                newMapButton.OnPressed += _ => PromptNewMap();
                box.AddChild(newMapButton);

                var loadMapButton = new Button()
                {
                    Text = "Load Map...",
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 4),
                };
                loadMapButton.OnPressed += _ => PromptLoadMap();
                box.AddChild(loadMapButton);

                var saveMapButton = new Button()
                {
                    Text = "Save Map...",
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 4),
                };
                saveMapButton.OnPressed += _ => PromptSaveMap();
                box.AddChild(saveMapButton);

                win.Contents.AddChild(box);
                this.OpenWindowAt(win, controlTestWindow.Rect.TopRight);
            }

            _tilePicker = new TilePickerWindow();
            _tilePicker.OnTileButtonPressed += SetSelectedTile;
            _tileBatch = new TileAtlasBatch(AtlasNames.Tile);

            _entityPicker = new EntityPickerWindow();
            _entityPicker.OnEntityButtonPressed += SetSelectedEntity;
            _chipBatch = new TileAtlasBatch(AtlasNames.Chip);

            _newMapDialog = new NewMapDialog();

            _initialized = true;
        }

        private TilePrototype? _selectedTile;
        private TilePickerWindow _tilePicker = default!;
        private bool CanPlaceTile => _selectedTile != null && _tilePicker.IsOpen;
        private TileAtlasBatch _tileBatch = default!;

        private EntityPrototype? _selectedEntity;
        private EntityPickerWindow _entityPicker = default!;
        private bool CanPlaceEntity => _selectedEntity != null && _entityPicker.IsOpen;
        private TileAtlasBatch _chipBatch = default!;
        private bool _placingTile = false;
        private Vector2i? _lastPlacedPos;

        private NewMapDialog _newMapDialog = default!;

        private void OpenTilePicker()
        {
            if (_tilePicker.IsOpen)
                _tilePicker.GrabFocus();
            else
                this.OpenWindowCentered(_tilePicker);
        }

        private void OpenEntityPicker()
        {
            if (_entityPicker.IsOpen)
                _entityPicker.GrabFocus();
            else
                this.OpenWindowCentered(_entityPicker);
        }

        private void PromptNewMap()
        {
            if (_newMapDialog.IsOpen)
                _newMapDialog.GrabFocus();
            else
                this.OpenWindowCentered(_newMapDialog);
        }

        private void PromptLoadMap()
        {
            var dir = Directory.GetCurrentDirectory();

            var result = NativeFileDialogSharp.Dialog.FileOpen("yml", dir);

            if (result.IsOk && result.Path.ToLowerInvariant().EndsWith(".yml"))
            {
                LoadMap(result.Path);
            }
        }

        private void PromptSaveMap()
        {
            var dir = Directory.GetCurrentDirectory();

            var result = NativeFileDialogSharp.Dialog.FileSave("yml", dir);

            if (result.IsOk)
            {
                SaveMap(result.Path);
            }
        }

        private void LoadMap(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                var map = _mapLoader.LoadBlueprint(reader);
                _entityManager.EnsureComponent<MapCommonComponent>(map.MapEntityUid).IsTemporary = true;

                var player = _gameSession.Player;
                var spatial = _entityManager.GetComponent<SpatialComponent>(player);
                EntitySystem.Get<IMapTransferSystem>().DoMapTransfer(spatial, map, new CenterMapLocation(), MapLoadType.Full);
            }
        }

        private void SaveMap(string path)
        {
            var map = _mapManager.ActiveMap!;
            var mapCommon = _entityManager.EnsureComponent<MapCommonComponent>(map.MapEntityUid);
            using (StreamWriter writer = File.CreateText(path))
            {
                mapCommon.IsTemporary = false;
                _mapLoader.SaveBlueprint(map.Id, writer);
                mapCommon.IsTemporary = true;
            }
        }

        private void SetSelectedTile(TilePickerWindow.TileButtonPressedEventArgs args)
        {
            SetSelectedTile(args.TilePrototype);
        }

        private void SetSelectedTile(TilePrototype tile)
        {
            _selectedTile = tile;
            _selectedEntity = null;
            _tileBatch.Clear();
            _tileBatch.Add(_coords.TileScale, _selectedTile.Image.AtlasIndex, 0, 0);
            _tileBatch.Flush();
        }

        private void SetSelectedEntity(EntityPickerWindow.EntityButtonPressedEventArgs args)
        {
            _selectedTile = null;
            _selectedEntity = args.EntityPrototype;
            _chipBatch.Clear();
            _chipBatch.Add(_coords.TileScale, args.ChipPrototype.Image.AtlasIndex, 0, 0, centering: BatchCentering.AlignBottom);
            _chipBatch.Flush();
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
                    _tileBatch.Draw(1, screenPos.X, screenPos.Y, color: new(1f, 1f, 1f, 0.25f));

                }
            }

            if (CanPlaceEntity)
            {
                if (_mapManager.ActiveMap != null)
                {
                    var tilePos = _field.Camera.VisibleScreenToTile(UserInterfaceManager.MousePositionScaled.Position);
                    var screenPos = _field.Camera.TileToVisibleScreen(_mapManager.ActiveMap.AtPos(tilePos));
                    _chipBatch.Draw(1, screenPos.X, screenPos.Y, color: new(1f, 1f, 1f, 0.25f));

                }
            }

            base.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (!CanPlaceTile && !CanPlaceEntity)
            {
                _placingTile = false;
                _lastPlacedPos = null;
            }

            if (_placingTile && _mapManager.ActiveMap != null)
            {
                var pos = _field.Camera.VisibleScreenToTile(UserInterfaceManager.MousePositionScaled.Position);

                if (_selectedTile != null)
                {
                    if (_selectedTile.IsSolid && _mapManager.ActiveMap.GetTilePrototype(pos) != _selectedTile)
                        _audio.Play(Protos.Sound.Offer1, pos);
                    _mapManager.ActiveMap.SetTile(pos, _selectedTile.GetStrongID());
                    _mapManager.ActiveMap.MemorizeTile(pos);
                }
                else if (_selectedEntity != null)
                {
                    if (_lastPlacedPos != pos)
                        EntitySystem.Get<IEntityGen>().SpawnEntity(_selectedEntity.GetStrongID(), _mapManager.ActiveMap.AtPos(pos));
                }

                _lastPlacedPos = pos;
                _field.RefreshScreen();
            }
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIClick)
            {
                _placingTile = true;
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIMiddleClick)
            {
                if (_mapManager.ActiveMap != null)
                {
                    var pos = _field.Camera.VisibleScreenToTile(args.PointerLocation.Position);
                    Logger.InfoS("debugview", $"{pos}");
                    OnRightClick(_mapManager.ActiveMap, pos);
                }
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIRightClick)
            {
                if (_mapManager.ActiveMap != null)
                {
                    var pos = _field.Camera.VisibleScreenToTile(args.PointerLocation.Position);
                    CopyTile(pos);
                }
                args.Handle();
            }
            else if (UiUtils.TryGetDirectionFromKeyFunction(args.Function, out var dir))
            {
                var spatial = _entityManager.GetComponent<SpatialComponent>(_gameSession.Player);
                var newCoords = spatial.LocalPosition + dir.Value.ToIntVec();
                if (_mapManager.ActiveMap!.IsInBounds(newCoords))
                {
                    spatial.Coordinates = new EntityCoordinates(spatial.ParentUid, newCoords);
                    _field.RefreshScreen();
                }
            }
        }

        private void CopyTile(Vector2i pos)
        {
            var tile = _mapManager.ActiveMap!.GetTilePrototype(pos);
            if (tile != null)
            {
                _audio.Play(Protos.Sound.Cursor1);
                SetSelectedTile(tile);
            }
        }

        private void HandleKeyBindUp(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UIClick)
            {
                _placingTile = false;
                args.Handle();
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
                _viewVariables.OpenVV(entity.Owner, this);
            }
        }
    }
}