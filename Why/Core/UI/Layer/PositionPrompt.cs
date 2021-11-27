using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Map;
using OpenNefia.Core.Object;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class PositionPrompt : BaseUiLayer<PositionPrompt.Result>
    {
        public new class Result
        {
            public TilePos Pos;
            public bool CanSee;

            public Result(TilePos pos, bool canSee)
            {
                Pos = pos;
                CanSee = canSee;
            }
        }

        TilePos OriginPos;
        TilePos TargetPos;
        Chara Onlooker;
        bool CanSee = false;
        bool IsPanning = false;

        ColorDef ColorTargetedTile;
        FontDef FontTargetText;
        IUiText TextTarget;

        public PositionPrompt(TilePos origin, TilePos? target = null, Chara? onlooker = null)
        {
            this.OriginPos = origin;
            this.TargetPos = target ?? origin;
            this.Onlooker = onlooker ?? Current.Player!;

            this.ColorTargetedTile = ColorDefOf.PromptTargetedTile;
            this.FontTargetText = FontDefOf.TargetText;
            this.TextTarget = new UiText(this.FontTargetText);

            this.BindKeys();
        }

        public PositionPrompt(Chara onlooker) : this(onlooker.GetTilePos()!.Value, onlooker: onlooker) { }

        protected virtual void BindKeys()
        {
            this.Keybinds[Keybind.Entries.Enter] += (_) => this.Finish(new Result(this.TargetPos, this.CanSee));
            this.Keybinds[Keybind.Entries.North] += (_) => this.MoveTargetPos(0, -1);
            this.Keybinds[Keybind.Entries.South] += (_) => this.MoveTargetPos(0, 1);
            this.Keybinds[Keybind.Entries.West] += (_) => this.MoveTargetPos(-1, 0);
            this.Keybinds[Keybind.Entries.East] += (_) => this.MoveTargetPos(1, 0);
            this.Keybinds[Keybind.Entries.Northwest] += (_) => this.MoveTargetPos(-1, -1);
            this.Keybinds[Keybind.Entries.Southwest] += (_) => this.MoveTargetPos(-1, 1);
            this.Keybinds[Keybind.Entries.Northeast] += (_) => this.MoveTargetPos(1, -1);
            this.Keybinds[Keybind.Entries.Southeast] += (_) => this.MoveTargetPos(1, 1);
            this.Keybinds[Keybind.Entries.Escape] += (_) => this.Cancel();
            this.Keybinds[Keybind.Entries.Cancel] += (_) => this.Cancel();
            this.Keybinds[Keybind.Entries.NextPage] += (_) => this.NextTarget();
            this.Keybinds[Keybind.Entries.PreviousPage] += (_) => this.PreviousTarget();

            this.MouseMoved.Callback += (evt) =>
            {
                if (this.IsPanning)
                    this.PanWithMouse(evt);
                else
                    this.MouseToTargetPos(evt.X, evt.Y);
            };
            this.MouseButtons[UI.MouseButtons.Mouse1] += (evt) => this.Finish(new Result(this.TargetPos, this.CanSee));
            this.MouseButtons[UI.MouseButtons.Mouse2].Bind((evt) => this.IsPanning = evt.State == KeyPressState.Pressed, trackReleased: true);
        }

        private void PanWithMouse(MouseMovedEvent evt)
        {
            Current.Field!.Camera.Pan(evt.Dx, evt.Dy);
        }

        private void MouseToTargetPos(int x, int y)
        {
            Current.Field!.Camera.VisibleScreenToTile(x, y, out var tx, out var ty);
            this.SetTargetPos(tx, ty);
        }

        protected void MoveTargetPos(int dx, int dy)
        {
            var tx = Math.Clamp(this.TargetPos.X + dx, 0, this.TargetPos.Map.Width - 1);
            var ty = Math.Clamp(this.TargetPos.Y + dy, 0, this.TargetPos.Map.Height - 1);
            this.SetTargetPos(tx, ty);
            this.UpdateCamera();
        }

        protected void SetTargetPos(int tx, int ty)
        {
            if (this.TargetPos.X == tx && this.TargetPos.Y == ty)
            {
                return;
            }

            this.TargetPos.X = tx;
            this.TargetPos.Y = ty;
            this.UpdateTargetText();
        }

        private void UpdateCamera()
        {
            Current.Field!.Camera.CenterOn(this.TargetPos);
        }

        private void UpdateTargetText()
        {
            this.CanSee = TargetText.GetTargetText(this.Onlooker, this.TargetPos, out var text, visibleOnly: true);
            this.TextTarget.Text = text;
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
            this.TextTarget.SetPosition(100, this.Height - Constants.INF_MSGH - 45 - this.TextTarget.Height);
        }

        public override void OnQuery()
        {
            this.UpdateCamera();
            this.UpdateTargetText();
        }

        public override void OnQueryFinish()
        {
            Current.Field!.RefreshScreen();
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
            Current.Field!.Camera.TileToVisibleScreen(this.TargetPos, out var sx, out var sy);
            var coords = GraphicsEx.Coords;
            Love.Graphics.SetBlendMode(BlendMode.Add);
            GraphicsEx.SetColor(this.ColorTargetedTile);
            Love.Graphics.Rectangle(DrawMode.Fill, sx, sy, coords.TileWidth, coords.TileHeight);

            if (this.ShouldDrawLine())
            {
                foreach (var (tx, ty) in PosUtils.EnumerateLine(OriginPos.X, OriginPos.Y, TargetPos.X, TargetPos.Y))
                {
                    Current.Field!.Camera.TileToVisibleScreen(this.TargetPos.Map.AtPos(tx, ty), out sx, out sy);
                    Love.Graphics.Rectangle(DrawMode.Fill, sx, sy, coords.TileWidth, coords.TileHeight);
                }
            }

            Love.Graphics.SetBlendMode(BlendMode.Alpha);

            this.TextTarget.Draw();
        }
    }
}
