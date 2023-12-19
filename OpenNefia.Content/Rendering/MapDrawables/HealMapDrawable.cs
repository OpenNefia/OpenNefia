using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.Rendering
{
    // >>>>>>>> shade2/screen.hsp:497: 	case aniCurse ..
    public sealed class HealMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        private readonly PrototypeId<SoundPrototype> _sound;
        private readonly IAssetInstance _asset;
        private readonly IList<Particle> _particles = new List<Particle>();
        private float _rotDelta = -1;

        protected struct Particle
        {
            public Particle(Vector2i pos, float rotationRads)
            {
                Pos = pos;
                RotationRads = rotationRads;
            }

            public Vector2i Pos;
            public float RotationRads;
        }

        protected FrameCounter Counter;

        public HealMapDrawable(PrototypeId<AssetPrototype> asset, PrototypeId<SoundPrototype> sound)
        {
            // TODO
            IoCManager.InjectDependencies(this);

            _sound = sound;
            _asset = Assets.Get(asset);

            var waitSecs = IoCManager.Resolve<IConfigurationManager>().GetCVar(CCVars.AnimeAnimationWait) / 5;

            Counter = new FrameCounter(waitSecs, 10);
        }

        public override void OnEnqueue()
        {
            _audio.Play(_sound, ScreenLocalPos); // TODO is this position correct?
            _particles.Clear();
            for (var i = 0; i < 15; i++)
            {
                var x = _rand.Next(_coords.TileSize.X);
                var y = _rand.Next(_coords.TileSize.Y) - 8;
                var rot = (_rand.Next(4) + 1) * _rotDelta;
                _particles.Add(new(new(x, y), rot));
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);
            foreach (var particle in _particles)
            {
                var frame2 = Counter.Frame * 2f - 1f;
                _asset.Draw(1f, 
                    X * _coords.TileScale + particle.Pos.X + (_coords.TileSize.X / 4) * _coords.TileScale, 
                    Y * _coords.TileScale + particle.Pos.Y / particle.RotationRads + (_coords.TileSize.Y / 4) * _coords.TileScale,
                    (_coords.TileSize.X - frame2 * 2f) * _coords.TileScale,
                    (_coords.TileSize.Y - frame2 * 2f) * _coords.TileScale,
                    centered: true, 
                    rotationRads: frame2 * particle.RotationRads);
            }
        }

        public override void Update(float dt)
        {
            Counter.Update(dt);
            if (Counter.IsFinished) // TODO finish immediately if off screen
                Finish();
        }
    }
    // <<<<<<<< shade2/screen.hsp:530 	swbreak ..
}
