using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
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

        [Obsolete("TODO remove as it is redundant with the position passed to IMapDrawablesManager.Enqueue()")]
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
            Color? color = null, 
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
            _counter = new FrameCounter(_config.GetCVar(CCVars.AnimeAnimationWait) / 5, (uint)maxFrames);
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
            var angle = -(float)Angle.BetweenPoints(_endPos.Position, _startPos.Position).Theta;
            var startPos = _coords.TileToScreen(_startPos.Position) + _coords.TileSize / 2;
            var endPos = _coords.TileToScreen(_endPos.Position) + _coords.TileSize / 2;
            var percent = _counter.Frame / _counter.MaxFrames;

            var cx = (int)MathHelper.Lerp(startPos.X, endPos.X, percent);
            var cy = (int)MathHelper.Lerp(startPos.Y, endPos.Y, percent);

            if (_graphics.IsPointInVisibleScreen(PixelPosition + new Vector2i(cx, cy)) || true)
            {
                _chipBatch.Clear();
                _chipBatch.Add(_coords.TileScale,
                    _chip.Image.AtlasIndex,
                    cx,
                    cy,
                    _coords.TileSize.X,
                    _coords.TileSize.Y,
                    color: _color,
                    centering: BatchCentering.Centered,
                    rotationRads: angle);
                _chipBatch.Flush();
                _chipBatch.Draw(1f, this.ScreenOffset.X, this.ScreenOffset.Y);
            }
        }

        public override void Dispose()
        {
            _chipBatch.Dispose();
        }
    }
}