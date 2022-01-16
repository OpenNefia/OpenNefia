using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.Rendering
{
    public class RangedAttackMapDrawable : BaseMapDrawable
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private MapCoordinates _startPos;
        private MapCoordinates _endPos;
        private ChipPrototype _chip;
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
            IoCManager.InjectDependencies(this);

            _startPos = startPos;
            _endPos = endPos;
            _chip = _protos.Index(chip);
            _color = color ?? Color.White;
            _sound = sound;
            _impactSound = impactSound;
            _chipBatch = new TileAtlasBatch(AtlasNames.Chip);

            var maxFrames = 0; 
            if (_startPos.TryDistanceTiled(_endPos, out var dist)) {
                maxFrames = (int)dist / 2 + 1;
            }
            _counter = new FrameCounter(_config.GetCVar(CCVars.AnimeWait), (uint)maxFrames);
        }

        public override void OnThemeSwitched()
        {
            base.OnThemeSwitched();

            _chipBatch.OnThemeSwitched();
        }

        public override bool CanEnqueue()
        {
            var playerSpatial = _entityManager.GetComponent<SpatialComponent>(_gameSession.Player);

            return _startPos.MapId == _endPos.MapId 
                && (Map.HasLineOfSight(playerSpatial.WorldPosition, _startPos.Position) 
                || Map.HasLineOfSight(playerSpatial.WorldPosition, _endPos.Position));
        }

        public override void OnEnqueue()
        {
            if (_sound != null)
            {
                _sounds.Play(_sound.Value, _startPos);
            }
        }

        public override void Update(float dt)
        {
            _counter.Update(dt);
            if (_counter.IsFinished)
            {
                if (_impactSound != null)
                {
                    _sounds.Play(_impactSound.Value, _endPos);
                }
                Finish();
            }
        }

        public override void Draw()
        {
            var screenPos = _coords.TileToScreen(_endPos.Position - _startPos.Position);
            var cx = (int)(_counter.Frame * (screenPos.X) / _counter.MaxFrames);
            var cy = (int)(_counter.Frame * (screenPos.Y) / _counter.MaxFrames);

            if (_graphics.IsPointInVisibleScreen(GlobalPixelPosition + new Vector2i(cx, cy)))
            {
                _chipBatch.Clear();
                _chipBatch.Add(_chip.Image.AtlasIndex, 
                    cx + _coords.TileSize.X / 2, 
                    cy + _coords.TileSize.Y / 2, 
                    _coords.TileSize.X,
                    _coords.TileSize.Y,
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
