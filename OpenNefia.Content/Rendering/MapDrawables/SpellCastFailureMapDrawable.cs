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
    // >>>>>>>> shade2/screen.hsp:478: 	case aniFizzle ..
    public sealed class SpellCastFailureMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        protected struct Particle
        {
            public Particle(Vector2i pos) { Pos = pos; }

            public Vector2i Pos;
        }

        protected IAssetInstance AssetFailureToCastEffect;
        protected FrameCounter Counter;

        public SpellCastFailureMapDrawable()
        {
            // TODO
            IoCManager.InjectDependencies(this);

            var waitSecs = IoCManager.Resolve<IConfigurationManager>().GetCVar(CCVars.AnimeAnimationWait) / 5;

            AssetFailureToCastEffect = Assets.Get(Protos.Asset.FailureToCastEffect);
            Counter = new FrameCounter(waitSecs, 12);
        }

        public override void OnEnqueue()
        {
            _audio.Play(Protos.Sound.Fizzle, ScreenLocalPos); // TODO is this position correct?
        }

        public override void Draw()
        {
            var scaling = (Counter.Frame + 40) / _coords.TileSize.X;
            var size = AssetFailureToCastEffect.PixelSize * scaling;
            Love.Graphics.SetColor(Color.White);
            AssetFailureToCastEffect.Draw(1f, X * _coords.TileScale + _coords.TileSize.X / 2, Y * _coords.TileScale  - _coords.TileSize.Y / 6 + _coords.TileSize.Y / 2,
                size.X * _coords.TileScale, size.Y * _coords.TileScale, centered: true, rotationRads: 10 * Counter.Frame);
        }

        public override void Update(float dt)
        {
            Counter.Update(dt);
            if (Counter.IsFinished) // TODO finish immediately if off screen
                Finish();
        }
    }
    // <<<<<<<< shade2/screen.hsp:495 	swbreak ..
}
