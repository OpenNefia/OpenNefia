using Love;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Config;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class RangedAttackMapDrawable : BaseMapDrawable
    {
        private MapCoordinates _startPos;
        private MapCoordinates _endPos;
        private PrototypeId<ChipPrototype> _chip;
        private Color _color;
        private PrototypeId<SoundPrototype>? _sound;
        private PrototypeId<SoundPrototype>? _impactSound;
        private TileAtlasBatch _chipBatch;
        private FrameCounter _counter;

        public RangedAttackMapDrawable(MapCoordinates startPos, MapCoordinates endPos, 
            PrototypeId<ChipPrototype> chip,
            Love.Color? color = null, 
            PrototypeId<SoundPrototype>? sound = null, 
            PrototypeId<SoundPrototype>? impactSound = null)
        {
            this._startPos = startPos;
            this._endPos = endPos;
            this._chip = chip;
            this._color = color ?? Love.Color.White;
            this._sound = sound;
            this._impactSound = sound;
            this._chipBatch = new TileAtlasBatch(AtlasNames.Chip);

            var maxFrames = PosHelpers.Distance(_startPos.Position, _endPos.Position) / 2 + 1;
            this._counter = new FrameCounter(ConfigVars.AnimeWait, (uint)maxFrames);
        }

        public override bool CanEnqueue()
        {
            return GameSession.Player.CanSee(this._startPos) || GameSession.Player.CanSee(this._endPos);
        }

        public override void OnEnqueue()
        {
            if (_sound != null)
            {
                Sounds.Play(_sound.Value, _startPos);
            }
        }

        public override void Update(float dt)
        {
            this._counter.Update(dt);
            if (this._counter.IsFinished)
            {
                if (this._impactSound != null)
                {
                    Sounds.Play(_impactSound.Value, _endPos);
                }
                this.Finish();
            }
        }

        public override void Draw()
        {
            var coords = GameSession.Coords;
            coords.TileToScreen(_endPos.Position - _startPos.Position, out var screenPos);
            var cx = (int)(this._counter.Frame * (screenPos.X) / this._counter.MaxFrames);
            var cy = (int)(this._counter.Frame * (screenPos.Y) / this._counter.MaxFrames);

            if (UiUtils.IsPointInVisibleScreen(this.Left + cx, this.Top + cy) || true)
            {
                this._chipBatch.Clear();
                this._chipBatch.Add(this._chip.Image, 
                    cx + coords.TileWidth / 2, 
                    cy + coords.TileHeight / 2, 
                    coords.TileWidth,
                    coords.TileHeight,
                    color: this._color, 
                    centered: true, 
                    rotation: this._startPos.AngleBetween(this._endPos));
                this._chipBatch.Flush();
                this._chipBatch.Draw(this.Left, this.Top, this.Width, this.Height);
            }
        }

        public override void Dispose()
        {
            this._chipBatch.Dispose();
        }
    }
}
