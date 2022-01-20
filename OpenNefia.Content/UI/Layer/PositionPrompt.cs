using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
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
    public class PositionPrompt : UiLayerWithResult<PositionPrompt.Args, PositionPrompt.Result>
    {
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly VisibilitySystem _visibility = default!;
        [Dependency] private readonly TargetTextSystem _targetText = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ICoords _coords = default!;

        public class Args
        {
            public MapCoordinates OriginPos { get; set; }
            public MapCoordinates TargetPos { get; set; }
            public EntityUid? Onlooker { get; set; }

            public Args(MapCoordinates origin, MapCoordinates? target = null, EntityUid? onlooker = null)
            {
                if (target != null && origin.MapId != target.Value.MapId)
                    target = origin;

                OriginPos = origin;
                TargetPos = target ?? origin;
                Onlooker = onlooker;
            }
        }

        public new class Result
        {
            public MapCoordinates Coords;
            public bool CanSee;

            public Result(MapCoordinates pos, bool canSee)
            {
                Coords = pos;
                CanSee = canSee;
            }
        }

        private MapCoordinates _originPos;
        private MapCoordinates _targetPos;
        private bool _canSee = false;
        private bool _isPanning = false;

        private EntityUid _onlooker = default!;
        private IMap _map = default!;

        protected Color ColorTargetedTile = UiColors.PromptTargetedTile;
        protected FontSpec FontTargetText = UiFonts.TargetText;
        [Child] protected UiText TextTarget;

        public PositionPrompt()
        {
            TextTarget = new UiTextOutlined(FontTargetText);

            OnKeyBindDown += HandleKeyBindDown;
            OnKeyBindUp += HandleKeyBindUp;
            EventFilter = UIEventFilterMode.Pass;
        }

        public override void Initialize(Args args)
        {
            _originPos = args.OriginPos;
            _targetPos = args.TargetPos;
            _onlooker = args.Onlooker ?? _gameSession.Player;
            _map = _mapManager.GetMap(args.OriginPos.MapId);
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UISelect || args.Function == EngineKeyFunctions.UIClick)
            {
                Finish(new Result(_targetPos, _canSee));
            }
            else if (args.Function.TryToDirection(out var dir))
            {
                MoveTargetPos(dir.ToIntVec());
            }
            else if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
            else if (args.Function == EngineKeyFunctions.UINextPage)
            {
                NextTarget();
            }
            else if (args.Function == EngineKeyFunctions.UIPreviousPage)
            {
                PreviousTarget();
            }
            else if (args.Function == EngineKeyFunctions.UIRightClick)
            {
                _isPanning = true;
            }
        }

        private void HandleKeyBindUp(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UIRightClick)
            {
                _isPanning = false;
            }
        }

        protected override void MouseMove(GUIMouseMoveEventArgs args)
        {
            if (_isPanning)
                PanWithMouse(args.Relative * UIScale);
            else
                MouseToTargetPos((Vector2i)args.GlobalPixelPosition.Position);
        }

        private void PanWithMouse(Vector2 delta)
        {
            _field.Camera.Pan(delta);
        }

        private void MouseToTargetPos(Vector2i screenPos)
        {
            var tilePos = _field.Camera.VisibleScreenToTile(screenPos);
            this.SetTargetPos(_mapManager.ActiveMap!.AtPos(tilePos));
        }

        protected void MoveTargetPos(Vector2i delta)
        {
            var tilePos = new Vector2i(Math.Clamp(_targetPos.X + delta.X, 0, _map.Width - 1),
                                       Math.Clamp(_targetPos.Y + delta.Y, 0, _map.Height - 1));

            SetTargetPos(_map.AtPos(tilePos));
            UpdateCamera();
        }

        protected void SetTargetPos(MapCoordinates tilePos)
        {
            if (_targetPos == tilePos)
            {
                return;
            }

            _targetPos = tilePos;
            UpdateTargetText();
        }

        private void UpdateCamera()
        {
            _field.Camera.CenterOnTilePos(_targetPos);
        }

        private void UpdateTargetText()
        {
            _canSee = _targetText.GetTargetText(this._onlooker, this._targetPos, out var text, visibleOnly: true);
            TextTarget.Text = text;
        }

        private void NextTarget()
        {

        }

        private void PreviousTarget()
        {

        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            TextTarget.SetPosition(100, Height - Constants.INF_MSGH - 45 - TextTarget.Height);
        }

        public override void OnQuery()
        {
            Sounds.Play(Sound.Pop2);

            UpdateCamera();
            UpdateTargetText();
        }

        public override void OnQueryFinish()
        {
            _field!.RefreshScreen();
        }

        public override void Update(float dt)
        {
            TextTarget.Update(dt);
        }

        private bool ShouldDrawLine()
        {
            var targetSpatial = _lookup.GetBlockingEntity(_targetPos);

            if (targetSpatial == null || !_visibility.CanSeeEntity(_onlooker, targetSpatial.Owner)
                || !_map.HasLineOfSight(targetSpatial.WorldPosition, _originPos.Position))
            {
                return false;
            }

            return _entityManager.HasComponent<MoveableComponent>(targetSpatial.Owner);
        }

        public override void Draw()
        {
            var screenPixelPos = _field.Camera.TileToVisibleScreen(_targetPos);
            Love.Graphics.SetBlendMode(Love.BlendMode.Add);
            GraphicsEx.SetColor(ColorTargetedTile);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, screenPixelPos.X, screenPixelPos.Y, _coords.TileSize.X, _coords.TileSize.Y);

            if (ShouldDrawLine())
            {
                foreach (var coords in PosHelpers.EnumerateLine(_originPos, _targetPos))
                {
                    screenPixelPos = _field.Camera.TileToVisibleScreen(coords);
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, screenPixelPos.X, screenPixelPos.Y, _coords.TileSize.X, _coords.TileSize.Y);
                }
            }

            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);

            TextTarget.Draw();
        }
    }
}
