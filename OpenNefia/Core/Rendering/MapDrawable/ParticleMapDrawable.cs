using OpenNefia.Core.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class ParticleMapDrawable : BaseMapDrawable
    {
        private struct Particle
        {
            public int X;
            public int Y;
            public float Rotation;

            public Particle(int x, int y, float rotation)
            {
                X = x;
                Y = y;
                Rotation = rotation;
            }
        }

        private AssetDrawable AssetParticle;
        private SoundDef? Sound;
        private float RotationVariance;
        private float AnimeWait;
        private Particle[] Particles;
        private FrameCounter Counter;

        public ParticleMapDrawable(AssetDef asset, SoundDef? sound, float rotationVariance = -1f, float? wait = null)
        {
            if (wait == null)
                wait = Config.AnimeWait;

            this.AssetParticle = new AssetDrawable(asset);
            this.Sound = sound;
            this.RotationVariance = rotationVariance;
            this.AnimeWait = wait.Value;
            var coords = GraphicsEx.Coords;
            this.Particles = Enumerable.Range(0, 15)
                .Select(_ => new Particle(Rand.NextInt(coords.TileWidth), 
                                          Rand.NextInt(coords.TileHeight),
                                          Rand.NextInt(4) + 1 * rotationVariance))
                .ToArray();
            this.Counter = new FrameCounter(wait.Value, 10);
        }

        public override void OnEnqueue()
        {
            if (this.Sound != null)
            {
                Sounds.PlayOneShot(this.Sound, this.ScreenLocalX, this.ScreenLocalY);
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
                this.AssetParticle.Draw(this.X + p.X, 
                    this.Y + p.Y + frame2 / p.Rotation, 
                    GraphicsEx.Coords.TileWidth - frame2 * 2,
                    GraphicsEx.Coords.TileHeight - frame2 * 2,
                    true,
                    frame2 * p.Rotation);
            }
        }

        public override void Dispose()
        {
            this.AssetParticle.Dispose();
        }
    }
}
