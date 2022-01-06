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

            _startPos = startPos;
            _endPos = endPos;
            _chip = chip.ResolvePrototype();
            _color = color ?? Color.White;
            _sound = sound?.ResolvePrototype();
            _impactSound = sound?.ResolvePrototype();
            _chipBatch = new TileAtlasBatch(AtlasNames.Chip);

            var maxFrames = 0; 
            if (_startPos.TryDistanceTiled(_endPos, out var dist)) {
                maxFrames = (int)dist / 2 + 1;
            }
            _counter = new FrameCounter(ConfigVars.AnimeWait, (uint)maxFrames);
        }

        public override void OnThemeSwitched()
        {
            base.OnThemeSwitched();

            _chipBatch.OnThemeSwitched();
        }

        public override bool CanEnqueue()
        {
            var playerSpatial = IoCManager.Resolve<IEntityManager>()
                .GetComponent<SpatialComponent>(GameSession.Player);

            return _startPos.MapId == _endPos.MapId 
                && (Map.HasLineOfSight(playerSpatial.WorldPosition, _startPos.Position) 
                || Map.HasLineOfSight(playerSpatial.WorldPosition, _endPos.Position));
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
            _counter.Update(dt);
            if (_counter.IsFinished)
            {
                if (_impactSound != null)
                {
                    Sounds.Play(_impactSound.GetStrongID(), _endPos);
                }
                Finish();
            }
        }

        public override void Draw()
        {
            var coords = GameSession.Coords;
            var screenPos = coords.TileToScreen(_endPos.Position - _startPos.Position);
            var cx = (int)(_counter.Frame * (screenPos.X) / _counter.MaxFrames);
            var cy = (int)(_counter.Frame * (screenPos.Y) / _counter.MaxFrames);

            if (_graphics.IsPointInVisibleScreen(GlobalPixelPosition + new Vector2i(cx, cy)))
            {
                _chipBatch.Clear();
                _chipBatch.Add(_chip.Image.AtlasIndex, 
                    cx + coords.TileSize.X / 2, 
                    cy + coords.TileSize.Y / 2, 
                    coords.TileSize.X,
                    coords.TileSize.Y,
                    color: _color, 
                    centered: true, 
                    rotation: (float)Angle.BetweenPoints(_startPos.Position, _endPos.Position).Degrees);
                _chipBatch.Flush();
                _chipBatch.Draw(X, Y, Width, Height);
            }
        }

        public override void Dispose()
        {
            _chipBatch.Dispose();
        }
    }
}
