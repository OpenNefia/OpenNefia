using OpenNefia.Core.Audio;
using OpenNefia.Core.Config;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;

namespace OpenNefia.Core.Rendering
{
    public class ParticleMapDrawable : BaseMapDrawable
    {
        private struct Particle
        {
            public Vector2i Pos;
            public float Rotation;

            public Particle(Vector2i pos, float rotation)
            {
                Pos = pos;
                Rotation = rotation;
            }
        }

        private IAssetDrawable AssetParticle;
        private PrototypeId<SoundPrototype>? Sound;
        private float RotationVariance;
        private float AnimeWait;
        private Particle[] Particles;
        private FrameCounter Counter;

        public ParticleMapDrawable(PrototypeId<AssetPrototype> asset, PrototypeId<SoundPrototype>? sound, float rotationVariance = -1f, float? wait = null)
        {
            if (wait == null)
                wait = ConfigVars.AnimeWait;

            this.AssetParticle = Assets.Get(asset);
            this.Sound = sound;
            this.RotationVariance = rotationVariance;
            this.AnimeWait = wait.Value;
            var coords = GameSession.Coords;
            this.Particles = Enumerable.Range(0, 15)
                .Select(_ => new Particle(new Vector2i(Rand.Next(coords.TileSize.X), Rand.Next(coords.TileSize.Y)),
                                          Rand.Next(4) + 1 * rotationVariance))
                .ToArray();
            this.Counter = new FrameCounter(wait.Value, 10);
        }

        public override void OnEnqueue()
        {
            if (this.Sound != null)
            {
                Sounds.Play(this.Sound.Value, this.ScreenLocalPos);
            }
        }

        public override void Update(float dt)
        {
            this.Counter.Update(dt);
            if (this.Counter.IsFinished)
            {
                this.Finish();
            }
        }

        public override void Draw()
        {
            var frame2 = this.Counter.Frame * 2f - 1f;
            Love.Graphics.SetColor(Love.Color.White);
            foreach (var p in this.Particles)
            {
                this.AssetParticle.Draw(this.X + p.Pos.X, 
                    this.Y + p.Pos.Y + frame2 / p.Rotation, 
                    GameSession.Coords.TileSize.X - frame2 * 2,
                    GameSession.Coords.TileSize.Y - frame2 * 2,
                    true,
                    frame2 * p.Rotation);
            }
        }

        public override void Dispose()
        {
        }
    }
}
