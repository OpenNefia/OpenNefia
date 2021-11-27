using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Map;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class RangedAttackMapDrawable : BaseMapDrawable
    {
        private TilePos StartPos;
        private TilePos EndPos;
        private ChipDef Chip;
        private Color Color;
        private SoundDef? Sound;
        private SoundDef? ImpactSound;
        private TileAtlasBatch ChipBatch;
        private FrameCounter Counter;

        public RangedAttackMapDrawable(TilePos startPos, TilePos endPos, ChipDef chip, Love.Color? color = null, SoundDef? sound = null, SoundDef? impactSound = null)
        {
            this.StartPos = startPos;
            this.EndPos = endPos;
            this.Chip = chip;
            this.Color = color ?? Love.Color.White;
            this.Sound = sound;
            this.ImpactSound = sound;
            this.ChipBatch = new TileAtlasBatch(Atlases.Chip);

            var maxFrames = this.StartPos.DistanceTo(this.EndPos) / 2 + 1;
            this.Counter = new FrameCounter(Config.AnimeWait, (uint)maxFrames);
        }

        public override bool CanEnqueue()
        {
            return Current.Player!.CanSee(this.StartPos) || Current.Player.CanSee(this.EndPos);
        }

        public override void OnEnqueue()
        {
            if (this.Sound != null)
            {
                Sounds.PlayOneShot(this.Sound, this.StartPos);
            }
        }

        public override void Update(float dt)
        {
            this.Counter.Update(dt);
            if (this.Counter.IsFinished)
            {
                if (this.ImpactSound != null)
                {
                    Sounds.PlayOneShot(this.ImpactSound, this.EndPos);
                }
                this.Finish();
            }
        }

        public override void Draw()
        {
            var coords = GraphicsEx.Coords;
            coords.TileToScreen(this.EndPos.X - StartPos.X, this.EndPos.Y - StartPos.Y, out var sx, out var sy);
            var cx = (int)(this.Counter.Frame * (sx) / this.Counter.MaxFrames);
            var cy = (int)(this.Counter.Frame * (sy) / this.Counter.MaxFrames);

            if (UiUtils.IsPointInVisibleScreen(this.X + cx, this.Y + cy) || true)
            {
                this.ChipBatch.Clear();
                this.ChipBatch.Add(this.Chip.Image, 
                    cx + coords.TileWidth / 2, 
                    cy + coords.TileHeight / 2, 
                    coords.TileWidth,
                    coords.TileHeight,
                    color: this.Color, 
                    centered: true, 
                    rotation: this.StartPos.AngleBetween(this.EndPos));
                this.ChipBatch.Flush();
                this.ChipBatch.Draw(this.X, this.Y, this.Width, this.Height);
            }
        }

        public override void Dispose()
        {
            this.ChipBatch.Dispose();
        }
    }
}
