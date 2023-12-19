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
using OpenNefia.Content.Visibility;

namespace OpenNefia.Content.Rendering
{
    // >>>>>>>> shade2/screen.hsp:588: 	case aniBolt ...
    public sealed class BallMapDrawable : BaseMapDrawable
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private readonly IList<MapCoordinates> _positions;
        private Color _color;
        private PrototypeId<SoundPrototype>? _impactSound;
        private FrameCounter _counter;
        private IAssetInstance _assetAnimBall;
        private IAssetInstance _assetAnimBall2;
        private int _endFrames = -1;
        private int _range;
        private Vector2i _tilePos;
        private int _frameIdx = 0;

        public BallMapDrawable(IList<MapCoordinates> positions, Color color, PrototypeId<SoundPrototype>? sound)
        {
            IoCManager.InjectDependencies(this);

            _positions = positions;
            _color = color.WithAlphaB(255);
            _impactSound = sound;
            _assetAnimBall = Assets.Get(Prototypes.Protos.Asset.AnimBall);
            _assetAnimBall2 = Assets.Get(Prototypes.Protos.Asset.AnimBall2);
            _counter = new FrameCounter(_config.GetCVar(CCVars.AnimeAnimationWait) / 5f, 10);
        }

        public override void OnEnqueue()
        {
            _sounds.Play(Prototypes.Protos.Sound.Ball1); // TODO positional
        }

        public override void Update(float dt)
        {
            _counter.Update(dt);
            if (_counter.IsFinished)
            {
                if (_impactSound != null && !IsFinished)
                    _sounds.Play(_impactSound.Value); // TODO positional
                Finish();
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(_color);
            foreach (var pos in _positions)
            {
                var screenPos = _coords.TileToScreen(pos.Position);
                _assetAnimBall2.DrawRegion(_coords.TileScale,
                    _counter.FrameInt.ToString(),
                    screenPos.X + ScreenOffset.X,
                    screenPos.Y + ScreenOffset.Y);
            }

            var alpha = (byte)int.Clamp(250 - _counter.FrameInt * _counter.FrameInt * 2, 0, 255);
            var screenPos2 = ScreenLocalPos;
            Love.Graphics.SetColor(_color.WithAlphaB(alpha));
            _assetAnimBall.DrawRegion(_coords.TileScale,
                _counter.FrameInt.ToString(),
                screenPos2.X - _coords.TileSize.X / 2 + ScreenOffset.X,
                screenPos2.Y - _coords.TileSize.Y / 2 + ScreenOffset.Y);
        }

        public override void Dispose()
        {
        }
    }
    // <<<<<<<< shade2/screen.hsp:621 	swbreak ...
}
