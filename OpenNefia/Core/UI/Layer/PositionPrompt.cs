using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.UI.Layer
{
    public class PositionPrompt : BaseUiLayer<PositionPrompt.Result>
    {
        public new class Result
        {
            public MapCoordinates Pos;
            public bool CanSee;

            public Result(MapCoordinates pos, bool canSee)
            {
                Pos = pos;
                CanSee = canSee;
            }
        }

        MapCoordinates OriginPos;
        MapCoordinates TargetPos;
        IEntity Onlooker;
        bool CanSee = false;
        bool IsPanning = false;

        [UiStyled] Color ColorTargetedTile;
        [UiStyled] FontSpec FontTargetText = new();
        IUiText TextTarget;

        public PositionPrompt(MapCoordinates origin, MapCoordinates? target = null, IEntity? onlooker = null)
        {
            this.OriginPos = origin;
            this.TargetPos = target ?? origin;
            this.Onlooker = onlooker ?? GameSession.Player!;

            this.TextTarget = new UiText(/*this.FontTargetText*/);

            this.BindKeys();
        }

        public PositionPrompt(IEntity onlooker) : this(onlooker.Coords, onlooker: onlooker) { }

        protected virtual void BindKeys()
        {
            this.Keybinds[CoreKeybinds.Enter] += (_) => this.Finish(new Result(this.TargetPos, this.CanSee));
            this.Keybinds[CoreKeybinds.North] += (_) => this.MoveTargetPos(0, -1);
            this.Keybinds[CoreKeybinds.South] += (_) => this.MoveTargetPos(0, 1);
            this.Keybinds[CoreKeybinds.West] += (_) => this.MoveTargetPos(-1, 0);
            this.Keybinds[CoreKeybinds.East] += (_) => this.MoveTargetPos(1, 0);
            this.Keybinds[CoreKeybinds.Northwest] += (_) => this.MoveTargetPos(-1, -1);
            this.Keybinds[CoreKeybinds.Southwest] += (_) => this.MoveTargetPos(-1, 1);
            this.Keybinds[CoreKeybinds.Northeast] += (_) => this.MoveTargetPos(1, -1);
            this.Keybinds[CoreKeybinds.Southeast] += (_) => this.MoveTargetPos(1, 1);
            this.Keybinds[CoreKeybinds.Escape] += (_) => this.Cancel();
            this.Keybinds[CoreKeybinds.Cancel] += (_) => this.Cancel();
            this.Keybinds[CoreKeybinds.NextPage] += (_) => this.NextTarget();
            this.Keybinds[CoreKeybinds.PreviousPage] += (_) => this.PreviousTarget();

            this.MouseMoved.Callback += (evt) =>
            {
                if (this.IsPanning)
                    this.PanWithMouse(evt);
                else
                    this.MouseToTargetPos(evt.Pos);
            };
            this.MouseButtons[UI.MouseButtons.Mouse1] += (evt) => this.Finish(new Result(this.TargetPos, this.CanSee));
            this.MouseButtons[UI.MouseButtons.Mouse2].Bind((evt) => this.IsPanning = evt.State == KeyPressState.Pressed, trackReleased: true);
        }

        private void PanWithMouse(UiMouseMovedEventArgs evt)
        {
            GameSession.Field.Camera.Pan(evt.DPos);
        }

        private void MouseToTargetPos(Vector2i screenPos)
        {
            GameSession.Field.Camera.VisibleScreenToTile(screenPos, out var tilePos);
            this.SetTargetPos(tilePos);
        }

        protected void MoveTargetPos(int dx, int dy)
        {
            var map = this.TargetPos.Map!;

            var tilePos = new Vector2i(Math.Clamp(this.TargetPos.X + dx, 0, map.Width - 1),
                                       Math.Clamp(this.TargetPos.Y + dy, 0, map.Height - 1));

            this.SetTargetPos(new MapCoordinates(map, tilePos));
            this.UpdateCamera();
        }

        protected void SetTargetPos(MapCoordinates tilePos)
        {
            if (this.TargetPos == tilePos)
            {
                return;
            }

            this.TargetPos = tilePos;
            this.UpdateTargetText();
        }

        private void UpdateCamera()
        {
            GameSession.Field.Camera.CenterOn(this.TargetPos);
        }

        private void UpdateTargetText()
        {
            this.CanSee = false; // TargetText.GetTargetText(this.Onlooker, this.TargetPos, out var text, visibleOnly: true);
            this.TextTarget.Text = "asdf"; //text;
        }

        private void NextTarget()
        {

        }

        private void PreviousTarget()
        {

        }

        public override void SetPosition(Vector2i pos)
        {
            base.SetPosition(pos);
            this.TextTarget.SetPosition(100, this.Height - Constants.INF_MSGH - 45 - this.TextTarget.Height);
        }

        public override void OnQuery()
        {
            this.UpdateCamera();
            this.UpdateTargetText();
        }

        public override void OnQueryFinish()
        {
            GameSession.Field!.RefreshScreen();
        }

        public override void Update(float dt)
        {
            this.TextTarget.Update(dt);
        }

        private bool ShouldDrawLine()
        {
            var targetChara = this.TargetPos.GetPrimaryChara();

            if (targetChara == null || !this.Onlooker.CanSee(targetChara) || !targetChara.HasLos(OriginPos))
            {
                return false;
            }

            return true;
        }

        public override void Draw()
        {
            GameSession.Field.Camera.TileToVisibleScreen(this.TargetPos, out var screenPos);
            Love.Graphics.SetBlendMode(BlendMode.Add);
            GraphicsEx.SetColor(this.ColorTargetedTile);
            Love.Graphics.Rectangle(DrawMode.Fill, screenPos.X, screenPos.Y, GameSession.Coords.TileSize.X, GameSession.Coords.TileSize.Y);

            if (this.ShouldDrawLine())
            {
                foreach (var coords in PosHelpers.EnumerateLine(OriginPos, TargetPos))
                {
                    GameSession.Field.Camera.TileToVisibleScreen(coords, out screenPos);
                    Love.Graphics.Rectangle(DrawMode.Fill, screenPos.X, screenPos.Y, GameSession.Coords.TileSize.X, GameSession.Coords.TileSize.Y);
                }
            }

            Love.Graphics.SetBlendMode(BlendMode.Alpha);

            this.TextTarget.Draw();
        }
    }
}
