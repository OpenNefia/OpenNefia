using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI;
using static OpenNefia.Content.Prototypes.Protos;
using Love;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.DebugView;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.RandomGen;

namespace OpenNefia.Content.Home
{
    public sealed class MapDesignerLayer : UiLayerWithResult<MapDesignerLayer.Args, UINone>
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ICoords _coords = default!;

        public override int? DefaultZOrder => HudLayer.HudZOrder + 10000;

        private SpatialComponent _target = default!;
        private IMap _map = default!;
        private bool _updateCursor = true;
        private Vector2i _prevPlayerPos;
        private TilePrototype _selectedTile = default!;
        private bool _placingTile = false;
        private TileAtlasBatch _tileBatch = default!;
        private Core.Maths.Vector2 _tileScreenPos;

        public MapDesignerLayer()
        {
            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
            OnKeyBindUp += HandleKeyBindUp;
        }

        public class Args
        {
            public Args(EntityUid target)
            {
                Target = target;
            }

            public EntityUid Target { get; }
        }

        public override void Initialize(Args args)
        {
            if (!_field.IsInGame())
            {
                Cancel();
                return;
            }

            _target = _entityManager.GetComponent<SpatialComponent>(args.Target);
            _map = _mapManager.GetMap(_target.MapID);
            _prevPlayerPos = _target.WorldPosition;
            _tileBatch = new TileAtlasBatch(AtlasNames.Tile);

            SetSelectedTile(_protos.Index(Protos.Tile.Grass));
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                _target.WorldPosition = _prevPlayerPos;
                _field.RefreshScreen();
                Cancel();
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UISelect)
            {
                PickTile();
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIClick)
            {
                _placingTile = true;
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
                MovePlayer(dir.Value);
                args.Handle();
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

        private void SetSelectedTile(TilePrototype proto)
        {
            _selectedTile = proto;
            _tileBatch.Clear();
            _tileBatch.Add(_coords.TileScale, _selectedTile.Image.AtlasIndex, 0, 0);
            _tileBatch.Flush();
        }

        private void MovePlayer(Direction dir)
        {
            var newPos = _target.WorldPosition + dir.ToIntVec();
            if (_map.IsInBounds(newPos))
            {
                _target.WorldPosition = newPos;
            }
            _field.RefreshScreen();
        }

        private void PickTile()
        {
            _placingTile = false;
            _updateCursor = false;
            var result = UserInterfaceManager.Query<TilePickerLayer, TilePickerLayer.Args, TilePickerLayer.Result>(new());
            _updateCursor = true;

            if (result.HasValue)
                SetSelectedTile(_protos.Index(result.Value.TileID));
        }

        public static bool IsTileSelectable(TilePrototype proto, IPrototypeManager protos)
        {
            var id = proto.GetStrongID();
            if (!protos.TryGetExtendedData(id, out ExtUsableInHomeDesigner? ext) || !ext.UsableInHomeDesigner)
            {
                return false;
            }

            if (proto.Kind == TileKind.Crop || proto.Kind == TileKind.Dryground)
            {
                return false;
            }

            return true;
        }

        private void CopyTile(Vector2i pos)
        {
            var tile = _map.GetTilePrototype(pos);
            if (tile != null && IsTileSelectable(tile, _protos))
            {
                _audio.Play(Protos.Sound.Cursor1);
                SetSelectedTile(tile);
            }
            else
            {
                _audio.Play(Protos.Sound.Fail1);
            }
        }

        public override void Update(float dt)
        {
            if (_updateCursor)
            {
                var tilePos = _field.Camera.VisibleScreenToTile(UserInterfaceManager.MousePositionScaled.Position);
                _tileScreenPos = _field.Camera.TileToVisibleScreen(_map.AtPos(tilePos));
            }

            if (_placingTile && _map != null)
            {
                var pos = _field.Camera.VisibleScreenToTile(UserInterfaceManager.MousePositionScaled.Position);

                if (_selectedTile != null)
                {
                    if (_selectedTile.IsSolid && _map.GetTilePrototype(pos) != _selectedTile)
                        _audio.Play(Protos.Sound.Offer1, new MapCoordinates(_map.Id, pos));
                    _map.SetTile(pos, _selectedTile.GetStrongID());
                    _map.MemorizeTile(pos);
                }

                _field.RefreshScreen();
            }
        }

        public override void Draw()
        {
            if (_map != null)
            {
                _tileBatch.Draw(1, _tileScreenPos.X, _tileScreenPos.Y, color: new(1f, 1f, 1f, 0.25f));
            }
        }
    }

    [DataDefinition]
    public sealed class ExtUsableInHomeDesigner : IPrototypeExtendedData<TilePrototype>
    {
        [DataField]
        public bool UsableInHomeDesigner { get; set; } = false;
    }
}
