using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Directions;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Content.Prototypes.Protos;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.UI.Layer
{
    public class DirectionPrompt : UiLayerWithResult<DirectionPrompt.Args, DirectionPrompt.Result>
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public class Args
        {
            public EntityCoordinates Origin { get; set; }
            public string? QueryText { get; set; }

            public Args(EntityCoordinates origin, string? prompt = null)
            {
                Origin = origin;
                QueryText = prompt;
            }

            public Args(EntityUid uid, string? prompt = null) : this(new EntityCoordinates(uid, Vector2i.Zero), prompt)
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
        private string? _queryText = null;
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
            _queryText = args.QueryText;
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
            else if (args.Function == EngineKeyFunctions.UISelect)
            {
                Finish(new Result(Direction.Center, _centerCoords.ToMap(_entityManager)));
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
                PanWithMouse(args.Relative * UIScale);
        }

        private void PanWithMouse(Vector2 delta)
        {
            _field.Camera.Pan(delta);
        }

        private void UpdateCamera()
        {
            _field.Camera.CenterOnTilePos(_centerCoords);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }

        public override void OnQuery()
        {
            Sounds.Play(Sound.Pop2);
            if (_queryText != null)
                _mes.Display(_queryText);

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
            var screenPos = _field.Camera.TileToVisibleScreen(_centerCoords) / UIScale;
            var (tileWidth, tileHeight) = _coords.TileSize / UIScale;

            var frame = _dt * 50;
            var alpha = (byte)Math.Max(200 - Math.Pow(frame / 2 % 20, 2), 0);

            var pos = Position + screenPos + (_coords.TileSize / UIScale) / 2;

            Love.Graphics.SetColor(Color.White.WithAlphaB(alpha));

            if (!_diagonalOnly)
            {
                AssetDirectionArrow.Draw(UIScale, pos.X, pos.Y - tileHeight, centered: true, rotationRads: Direction.North.ToAngle());
                AssetDirectionArrow.Draw(UIScale, pos.X, pos.Y + tileHeight, centered: true, rotationRads: Direction.South.ToAngle());
                AssetDirectionArrow.Draw(UIScale, pos.X + tileWidth, pos.Y, centered: true, rotationRads: Direction.East.ToAngle());
                AssetDirectionArrow.Draw(UIScale, pos.X - tileWidth, pos.Y, centered: true, rotationRads: Direction.West.ToAngle());
            }

            AssetDirectionArrow.Draw(UIScale, pos.X - tileWidth, pos.Y - tileHeight, centered: true, rotationRads: Direction.NorthWest.ToAngle());
            AssetDirectionArrow.Draw(UIScale, pos.X + tileWidth, pos.Y + tileHeight, centered: true, rotationRads: Direction.SouthEast.ToAngle());
            AssetDirectionArrow.Draw(UIScale, pos.X + tileWidth, pos.Y - tileHeight, centered: true, rotationRads: Direction.NorthEast.ToAngle());
            AssetDirectionArrow.Draw(UIScale, pos.X - tileWidth, pos.Y + tileHeight, centered: true, rotationRads: Direction.SouthWest.ToAngle());
        }
    }
}
