using Love;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Data.Types;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = OpenNefia.Core.Maths.Color;
using MouseButton = OpenNefia.Core.UI.MouseButton;

namespace OpenNefia.Content.UI.Layer
{
    public class PositionPrompt : BaseUiLayer<PositionPrompt.Result>
    {
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

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

        MapCoordinates OriginPos;
        MapCoordinates TargetPos;
        Entity Onlooker;
        bool CanSee = false;
        bool IsPanning = false;

        Color ColorTargetedTile = UiColors.PromptTargetedTile;
        FontSpec FontTargetText = UiFonts.TargetText;
        IUiText TextTarget;

        public PositionPrompt(MapCoordinates origin, MapCoordinates? target = null, Entity? onlooker = null)
        {
            IoCManager.InjectDependencies(this);

            OriginPos = origin;
            TargetPos = target ?? origin;
            Onlooker = onlooker ?? GameSession.Player!;

            TextTarget = new UiTextOutlined(FontTargetText);

            BindKeys();
        }

        public PositionPrompt(Entity onlooker, MapCoordinates? target)
            : this(onlooker.Spatial.Coords, target, onlooker) { }

        public PositionPrompt(Entity onlooker) 
            : this(onlooker.Spatial.Coords, onlooker: onlooker) { }

        protected virtual void BindKeys()
        {
            Keybinds[CoreKeybinds.Enter] += (_) => Finish(new Result(TargetPos, CanSee));
            Keybinds[CoreKeybinds.North] += (_) => MoveTargetPos(0, -1);
            Keybinds[CoreKeybinds.South] += (_) => MoveTargetPos(0, 1);
            Keybinds[CoreKeybinds.West] += (_) => MoveTargetPos(-1, 0);
            Keybinds[CoreKeybinds.East] += (_) => MoveTargetPos(1, 0);
            Keybinds[CoreKeybinds.Northwest] += (_) => MoveTargetPos(-1, -1);
            Keybinds[CoreKeybinds.Southwest] += (_) => MoveTargetPos(-1, 1);
            Keybinds[CoreKeybinds.Northeast] += (_) => MoveTargetPos(1, -1);
            Keybinds[CoreKeybinds.Southeast] += (_) => MoveTargetPos(1, 1);
            Keybinds[CoreKeybinds.Escape] += (_) => Cancel();
            Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();
            Keybinds[CoreKeybinds.NextPage] += (_) => NextTarget();
            Keybinds[CoreKeybinds.PreviousPage] += (_) => PreviousTarget();

            MouseMoved.Callback += (evt) =>
            {
                if (IsPanning)
                    PanWithMouse(evt);
                else
                    MouseToTargetPos(evt.Pos);
            };
            MouseButtons[MouseButton.Mouse1] += (evt) => this.Finish(new Result(this.TargetPos, this.CanSee));
            MouseButtons[MouseButton.Mouse2].Bind((evt) => IsPanning = evt.State == KeyPressState.Pressed, trackReleased: true);
        }

        private void PanWithMouse(UiMouseMovedEventArgs evt)
        {
            _field.Camera.Pan(evt.DPos);
        }

        private void MouseToTargetPos(Vector2i screenPos)
        {
            _field.Camera.VisibleScreenToTile(screenPos, out var tilePos);
            this.SetTargetPos(_mapManager.ActiveMap!.AtPos(tilePos));
        }

        protected void MoveTargetPos(int dx, int dy)
        {
            var map = TargetPos.Map!;

            var tilePos = new Vector2i(Math.Clamp(TargetPos.X + dx, 0, map.Width - 1),
                                       Math.Clamp(TargetPos.Y + dy, 0, map.Height - 1));

            SetTargetPos(new MapCoordinates(map, tilePos));
            UpdateCamera();
        }

        protected void SetTargetPos(MapCoordinates tilePos)
        {
            if (TargetPos == tilePos)
            {
                return;
            }

            TargetPos = tilePos;
            UpdateTargetText();
        }

        private void UpdateCamera()
        {
            _field.Camera.CenterOnTilePos(TargetPos);
        }

        private void UpdateTargetText()
        {
            CanSee = false; // TargetText.GetTargetText(this.Onlooker, this.TargetPos, out var text, visibleOnly: true);
            TextTarget.Text = "asdf"; //text;
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
            var targetChara = TargetPos.GetPrimaryChara();

            if (targetChara == null || !Onlooker.CanSee(targetChara) || !targetChara.HasLos(OriginPos))
            {
                return false;
            }

            return true;
        }

        public override void Draw()
        {
            _field.Camera.TileToVisibleScreen(TargetPos, out var screenPos);
            Graphics.SetBlendMode(BlendMode.Add);
            GraphicsEx.SetColor(ColorTargetedTile);
            Graphics.Rectangle(DrawMode.Fill, screenPos.X, screenPos.Y, GameSession.Coords.TileSize.X, GameSession.Coords.TileSize.Y);

            if (ShouldDrawLine())
            {
                foreach (var coords in PosHelpers.EnumerateLine(OriginPos, TargetPos))
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
