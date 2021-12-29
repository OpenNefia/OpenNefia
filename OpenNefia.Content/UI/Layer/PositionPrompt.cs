using Love;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.UI.Layer
{
    public class PositionPrompt : UiLayerWithResult<PositionPrompt.Result>
    {
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly VisibilitySystem _visibility = default!;
        [Dependency] private readonly TargetTextSystem _targetText = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

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
        private Entity _onlooker;
        private bool _canSee = false;
        private bool _isPanning = false;
        private IMap _map;

        protected Color ColorTargetedTile = UiColors.PromptTargetedTile;
        protected FontSpec FontTargetText = UiFonts.TargetText;
        protected IUiText TextTarget;

        public PositionPrompt(MapCoordinates origin, MapCoordinates? target = null, Entity? onlooker = null)
        {
            EntitySystem.InjectDependencies(this);

            if (target != null && origin.MapId != target.Value.MapId)
                target = origin;

            _originPos = origin;
            _targetPos = target ?? origin;
            _onlooker = onlooker ?? GameSession.Player!;
            _map = _mapManager.GetMap(origin.MapId);

            TextTarget = new UiTextOutlined(FontTargetText);

            BindKeys();
        }

        public PositionPrompt(Entity onlooker, MapCoordinates? target)
            : this(onlooker.Spatial.MapPosition, target, onlooker) { }

        public PositionPrompt(Entity onlooker) 
            : this(onlooker.Spatial.MapPosition, onlooker: onlooker) { }

        protected virtual void BindKeys()
        {
            //Keybinds[CoreKeybinds.Enter] += (_) => Finish(new Result(_targetPos, _canSee));
            //Keybinds[CoreKeybinds.North] += (_) => MoveTargetPos(0, -1);
            //Keybinds[CoreKeybinds.South] += (_) => MoveTargetPos(0, 1);
            //Keybinds[CoreKeybinds.West] += (_) => MoveTargetPos(-1, 0);
            //Keybinds[CoreKeybinds.East] += (_) => MoveTargetPos(1, 0);
            //Keybinds[CoreKeybinds.Northwest] += (_) => MoveTargetPos(-1, -1);
            //Keybinds[CoreKeybinds.Southwest] += (_) => MoveTargetPos(-1, 1);
            //Keybinds[CoreKeybinds.Northeast] += (_) => MoveTargetPos(1, -1);
            //Keybinds[CoreKeybinds.Southeast] += (_) => MoveTargetPos(1, 1);
            //Keybinds[CoreKeybinds.Escape] += (_) => Cancel();
            //Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();
            //Keybinds[CoreKeybinds.NextPage] += (_) => NextTarget();
            //Keybinds[CoreKeybinds.PreviousPage] += (_) => PreviousTarget();

            //MouseMoved.Callback += (evt) =>
            //{
            //    if (_isPanning)
            //        PanWithMouse(evt);
            //    else
            //        MouseToTargetPos(evt.Pos);
            //};
            //MouseButtons[MouseButton.Mouse1] += (evt) => this.Finish(new Result(this._targetPos, this._canSee));
            //MouseButtons[MouseButton.Mouse2].Bind((evt) => _isPanning = evt.State == KeyPressState.Pressed, trackReleased: true);
        }

        private void PanWithMouse(GUIMouseMoveEventArgs evt)
        {
            _field.Camera.Pan(evt.Relative);
        }

        private void MouseToTargetPos(Vector2i screenPos)
        {
            _field.Camera.VisibleScreenToTile(screenPos, out var tilePos);
            this.SetTargetPos(_mapManager.ActiveMap!.AtPos(tilePos));
        }

        protected void MoveTargetPos(int dx, int dy)
        {
            var tilePos = new Vector2i(Math.Clamp(_targetPos.X + dx, 0, _map.Width - 1),
                                       Math.Clamp(_targetPos.Y + dy, 0, _map.Height - 1));

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
            _canSee = _targetText.GetTargetText(this._onlooker.Uid, this._targetPos, out var text, visibleOnly: true);
            TextTarget.Text = text;
        }

        private void NextTarget()
        {

        }

        private void PreviousTarget()
        {

        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            TextTarget.SetPosition(100, Height - Constants.INF_MSGH - 45 - TextTarget.Height);
        }

        public override void OnQuery()
        {
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
            var targetChara = _lookup.GetPrimaryEntity(_targetPos);

            if (targetChara == null || !_visibility.CanSeeEntity(targetChara.Uid) 
                || !_map.HasLos(targetChara.Spatial.WorldPosition, _originPos.Position))
            {
                return false;
            }

            return _entityManager.HasComponent<MoveableComponent>(targetChara.Uid);
        }

        public override void Draw()
        {
            _field.Camera.TileToVisibleScreen(_targetPos, out var screenPos);
            Graphics.SetBlendMode(BlendMode.Add);
            GraphicsEx.SetColor(ColorTargetedTile);
            Graphics.Rectangle(DrawMode.Fill, screenPos.X, screenPos.Y, GameSession.Coords.TileSize.X, GameSession.Coords.TileSize.Y);

            if (ShouldDrawLine())
            {
                foreach (var coords in PosHelpers.EnumerateLine(_originPos, _targetPos))
                {
                    _field.Camera.TileToVisibleScreen(coords, out screenPos);
                    Graphics.Rectangle(DrawMode.Fill, screenPos.X, screenPos.Y, GameSession.Coords.TileSize.X, GameSession.Coords.TileSize.Y);
                }
            }

            Graphics.SetBlendMode(BlendMode.Alpha);

            TextTarget.Draw();
        }
    }
}
