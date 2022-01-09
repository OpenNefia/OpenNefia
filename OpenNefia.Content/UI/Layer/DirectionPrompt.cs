using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Input;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Directions;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Utility;
using static OpenNefia.Content.Prototypes.Protos;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.UI.Layer
{
    public class DirectionPrompt : UiLayerWithResult<DirectionPrompt.Args, DirectionPrompt.Result>
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly ICoords _coords = default!;

        public class Args
        {
            public EntityCoordinates Origin { get; set; }

            public Args(EntityCoordinates origin)
            {
                Origin = origin;
            }

            public Args(EntityUid uid) : this(new EntityCoordinates(uid, Vector2i.Zero))
            {
            }
        }
        public new class Result
        {
            public Direction Direction;
            public MapCoordinates Coords;

            public Result(Direction direction, MapCoordinates coords)
            {
                Direction = direction;
                Coords = coords;
            }
        }

        private EntityCoordinates _centerCoords;
        private bool _isPanning = false;
        private float _dt;
        private bool _diagonalOnly = false;

        private IAssetInstance AssetDirectionArrow;

        public DirectionPrompt()
        {
            AssetDirectionArrow = Assets.Get(Protos.Asset.DirectionArrow);

            OnKeyBindDown += HandleKeyBindDown;
            OnKeyBindUp += HandleKeyBindUp;
            EventFilter = UIEventFilterMode.Pass;
        }

        public override void Initialize(Args args)
        {
            _centerCoords = args.Origin;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function.TryToDirection(out var dir))
            {
                if (_diagonalOnly && dir.IsCardinal())
                    return;

                var mapCoords = _centerCoords.Offset(dir).ToMap(_entityManager);
                Finish(new Result(dir, mapCoords));
            }
            else if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
            else if (args.Function == EngineKeyFunctions.UIRightClick)
            {
                _isPanning = true;
            }
            else if (args.Function == ContentKeyFunctions.DiagonalOnly)
            {
                _diagonalOnly = true;
            }
        }

        private void HandleKeyBindUp(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UIRightClick)
            {
                _isPanning = false;
            }
            else if (args.Function == ContentKeyFunctions.DiagonalOnly)
            {
                _diagonalOnly = false;
            }
        }

        protected override void MouseMove(GUIMouseMoveEventArgs args)
        {
            if (_isPanning)
                PanWithMouse(args.Relative);
        }

        private void PanWithMouse(Vector2 delta)
        {
            _field.Camera.Pan(delta);
        }

        private void UpdateCamera()
        {
            _field.Camera.CenterOnTilePos(_centerCoords);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
        }

        public override void OnQuery()
        {
            Sounds.Play(Sound.Pop2);

            UpdateCamera();
        }

        public override void OnQueryFinish()
        {
            _field!.RefreshScreen();
        }

        public override void Update(float dt)
        {
            _dt += dt;

        }

        public override void Draw()
        {
            var screenPos = _field.Camera.TileToVisibleScreen(_centerCoords);
            var (tileWidth, tileHeight) = _coords.TileSize;

            var frame = _dt * 50;
            var alpha = (byte)Math.Max(200 - Math.Pow(frame / 2 % 20, 2), 0);

            var pos = GlobalPixelPosition + screenPos + _coords.TileSize / 2;

            Love.Graphics.SetColor(Color.White.WithAlpha(alpha));

            if (!_diagonalOnly)
            {
                AssetDirectionArrow.Draw(pos.X, pos.Y - tileHeight, centered: true, rotation: Direction.North.ToAngle());
                AssetDirectionArrow.Draw(pos.X, pos.Y + tileHeight, centered: true, rotation: Direction.South.ToAngle());
                AssetDirectionArrow.Draw(pos.X + tileWidth, pos.Y, centered: true, rotation: Direction.East.ToAngle());
                AssetDirectionArrow.Draw(pos.X - tileWidth, pos.Y, centered: true, rotation: Direction.West.ToAngle());
            }

            AssetDirectionArrow.Draw(pos.X - tileWidth, pos.Y - tileHeight, centered: true, rotation: Direction.NorthWest.ToAngle());
            AssetDirectionArrow.Draw(pos.X + tileWidth, pos.Y + tileHeight, centered: true, rotation: Direction.SouthEast.ToAngle());
            AssetDirectionArrow.Draw(pos.X + tileWidth, pos.Y - tileHeight, centered: true, rotation: Direction.NorthEast.ToAngle());
            AssetDirectionArrow.Draw(pos.X - tileWidth, pos.Y + tileHeight, centered: true, rotation: Direction.SouthWest.ToAngle());
        }
    }
}
