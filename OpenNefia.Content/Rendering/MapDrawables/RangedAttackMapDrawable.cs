using Love;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Config;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.Rendering
{
    public class RangedAttackMapDrawable : BaseMapDrawable
    {
        [Dependency] private readonly IGraphics _graphics = default!;

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
            IoCManager.InjectDependencies(this);

            this._startPos = startPos;
            this._endPos = endPos;
            this._chip = chip.ResolvePrototype();
            this._color = color ?? Color.White;
            this._sound = sound?.ResolvePrototype();
            this._impactSound = sound?.ResolvePrototype();
            this._chipBatch = new TileAtlasBatch(AtlasNames.Chip);

            var maxFrames = 0; 
            if (_startPos.TryDistance(_endPos, out var dist)) {
                maxFrames = (int)dist / 2 + 1;
            }
            this._counter = new FrameCounter(ConfigVars.AnimeWait, (uint)maxFrames);
        }

        public override void OnThemeSwitched()
        {
            base.OnThemeSwitched();

            _chipBatch.OnThemeSwitched();
        }

        public override bool CanEnqueue()
        {
            return _startPos.MapId == _endPos.MapId 
                && (Map.HasLineOfSight(GameSession.Player.Spatial.WorldPosition, this._startPos.Position) 
                || Map.HasLineOfSight(GameSession.Player.Spatial.WorldPosition, this._endPos.Position));
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

            if (_graphics.IsPointInVisibleScreen(this.GlobalPixelPosition + new Vector2i(cx, cy)))
            {
                this._chipBatch.Clear();
                this._chipBatch.Add(this._chip.Image.AtlasIndex, 
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
