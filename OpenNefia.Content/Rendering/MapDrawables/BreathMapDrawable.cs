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
    // >>>>>>>> shade2/screen.hsp:532: 	case aniBreath ..
    public sealed class BreathMapDrawable : BaseMapDrawable
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private readonly IList<MapCoordinates> _positions;
        private readonly MapCoordinates _startCoords;
        private readonly MapCoordinates _targetCoords;
        private Color _color;
        private PrototypeId<SoundPrototype>? _impactSound;
        private FrameCounter _counter;
        private IAssetInstance _assetAnimBreath;

        public BreathMapDrawable(IList<MapCoordinates> positions, MapCoordinates startCoords, MapCoordinates targetCoords, Color color, PrototypeId<SoundPrototype>? sound)
        {
            IoCManager.InjectDependencies(this);

            _positions = positions;
            _startCoords = startCoords; // TODO remove
            _targetCoords = targetCoords;
            _color = color.WithAlphaB(255);
            _impactSound = sound;
            _assetAnimBreath = Assets.Get(Prototypes.Protos.Asset.AnimBreath);
            _counter = new FrameCounter(_config.GetCVar(CCVars.AnimeAnimationWait) / 5f, 6);
        }

        public override void OnEnqueue()
        {
            _sounds.Play(Prototypes.Protos.Sound.Breath1); // TODO positional
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
            var angle = (float)Angle.BetweenPoints(_targetCoords.Position, _startCoords.Position).Theta;

            foreach (var pos in _positions)
            {
                var screenPos = _coords.TileToScreen(pos.Position);
                _assetAnimBreath.DrawRegion(_coords.TileScale,
                    _counter.FrameInt.ToString(),
                    screenPos.X + ScreenOffset.X,
                    screenPos.Y + ScreenOffset.Y);
            }
        }
    }
    // <<<<<<<< shade2/screen.hsp:551 	swbreak ..
}
