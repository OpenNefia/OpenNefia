using Love;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Config;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.Rendering
{
    public class RangedAttackMapDrawable : BaseMapDrawable
    {
        private MapCoordinates _startPos;
        private MapCoordinates _endPos;
        private ChipPrototype _chip;
        private Color _color;
        private SoundPrototype? _sound;
        private SoundPrototype? _impactSound;
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
            this._chip = chip.ResolvePrototype();
            this._color = color ?? Color.White;
            this._sound = sound?.ResolvePrototype();
            this._impactSound = sound?.ResolvePrototype();
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
                Sounds.Play(_sound.GetStrongID(), _startPos);
            }
        }

        public override void Update(float dt)
        {
            this._counter.Update(dt);
            if (this._counter.IsFinished)
            {
                if (this._impactSound != null)
                {
                    Sounds.Play(_impactSound.GetStrongID(), _endPos);
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

            if (UiUtils.IsPointInVisibleScreen(this.Position))
            {
                this._chipBatch.Clear();
                this._chipBatch.Add(this._chip.Image, 
                    cx + coords.TileSize.X / 2, 
                    cy + coords.TileSize.Y / 2, 
                    coords.TileSize.X,
                    coords.TileSize.Y,
                    color: this._color, 
                    centered: true, 
                    rotation: (float)Angle.BetweenPoints(this._startPos.Position, this._endPos.Position).Degrees);
                this._chipBatch.Flush();
                this._chipBatch.Draw(this.X, this.Y, this.Width, this.Height);
            }
        }

        public override void Dispose()
        {
            this._chipBatch.Dispose();
        }
    }
}
