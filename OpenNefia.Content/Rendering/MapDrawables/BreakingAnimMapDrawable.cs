using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Rendering
{
    public abstract class BreakingAnimMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;

        protected struct Particle
        {
            public Particle(Vector2i pos) { Pos = pos; }

            public Vector2i Pos;
        }

        protected IAssetInstance AssetParticle;
        protected Particle[] Particles;
        protected FrameCounter Counter;

        public BreakingAnimMapDrawable(PrototypeId<AssetPrototype> asset, uint duration, int maxParticles,float? waitSecs = null)
        {
            // TODO
            IoCManager.InjectDependencies(this);

            if (waitSecs == null)
                waitSecs = IoCManager.Resolve<IConfigurationManager>().GetCVar(CCVars.AnimeAnimationWait);

            AssetParticle = Assets.Get(asset);
            Particles = Enumerable.Range(0, maxParticles)
                .Select(_ => CreateParticle())
                .ToArray();
            Counter = new FrameCounter(waitSecs.Value, duration);
        }

        protected abstract Particle CreateParticle();
        protected abstract void DrawParticle(Vector2i pixelPos, float frame, Vector2 particlePixelPos, int particleIndex);

        public override void Draw()
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                var particle = Particles[i];
                DrawParticle(PixelPosition, Counter.Frame, particle.Pos, i);
            }
        }
        public override void Update(float dt)
        {
            Counter.Update(dt);
            if (Counter.IsFinished)
            {
                Finish();
            }
        }
    }

    public class BreakingFragmentsMapDrawable : BreakingAnimMapDrawable
    {
        public BreakingFragmentsMapDrawable(float? waitSecs = null) : base(Protos.Asset.BreakingEffect, 5, 4, waitSecs)
        {
        }

        protected override Particle CreateParticle()
        {
            return new Particle((_rand.Next(24) - 12, _rand.Next(8)));
        }

        protected override void DrawParticle(Vector2i pixelPos, float frame, Vector2 particlePixelPos, int particleIndex)
        {
            var x = pixelPos.X + particlePixelPos.X;
            var add = 0f;
            if ((particleIndex % 2) == 0)
            {
                add = 1f;
            }

            if (particlePixelPos.X < 4)
            {
                x -= (1f + add) * frame;
            }
            if (particlePixelPos.X > -4)
            {
                x += (1f + add) * frame;
            }

            var y = pixelPos.Y - _coords.TileSize.Y / 4 + particlePixelPos.Y + frame * frame / 3;

            var size = AssetParticle.PixelSize / 2;
            AssetParticle.DrawUnscaled(x, y, size.X, size.Y, centered: true, rotationRads: (float)Angle.FromDegrees(particleIndex * 23f).Theta);
        }
    }
}
