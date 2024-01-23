using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Graphics;

namespace OpenNefia.Content.Rendering
{
    // >>>>>>>> shade2/screen.hsp:829 	case aniMeteor ...
    public sealed class MeteorMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        private readonly IAssetInstance _assetMeteor;
        private readonly IAssetInstance _assetMeteorImpact;
        private readonly IList<Meteor> _meteors = new List<Meteor>();

        protected class Meteor
        {
            public Meteor(Vector2 pos, float frame)
            {
                Pos = pos;
                Frame = frame;
            }

            public Vector2 Pos;
            public float Frame;
        }

        protected FrameCounter Counter;

        public MeteorMapDrawable()
        {
            // TODO
            IoCManager.InjectDependencies(this);

            _assetMeteor = Assets.Get(Protos.Asset.AnimMeteor);
            _assetMeteorImpact = Assets.Get(Protos.Asset.AnimMeteorImpact);

            var waitSecs = IoCManager.Resolve<IConfigurationManager>().GetCVar(CCVars.AnimeAnimationWait) / 2;
            Counter = new FrameCounter(waitSecs);
        }

        public override void OnEnqueue()
        {
            _audio.Play(Protos.Sound.AtkFire, ScreenLocalPos); // TODO is this position correct?
            _meteors.Clear();
            for (var i = 0; i < 75; i++)
            {
                var x = (240 + _rand.Next(_graphics.WindowPixelSize.X)) / _coords.TileScale;
                var y = -96;
                var frame = _rand.Next(8);
                _meteors.Add(new(new(x, y), frame));
            }
        }

        private bool _didAnything = true;

        public override void Draw()
        {
            // TODO screen shake

            Love.Graphics.SetColor(Color.White);

            _didAnything = false;

            foreach (var meteor in _meteors)
            {
                if (meteor.Frame < 16)
                {
                    _didAnything = true;

                    if (meteor.Frame >= 10)
                    {
                        // impact
                        var frame = int.Clamp((int)float.Floor(meteor.Frame) - 10, 0, 4);
                        _assetMeteorImpact.DrawRegion(_coords.TileScale, frame.ToString(), meteor.Pos.X - 48, meteor.Pos.Y);
                    }
                    if (meteor.Frame < 16)
                    {
                        // meteor disintegration
                        var frame = int.Clamp((int)float.Floor(meteor.Frame) - 8, 0, 8);
                        _assetMeteor.DrawRegion(_coords.TileScale, frame.ToString(), meteor.Pos.X, meteor.Pos.Y);
                    }
                }
            }

            var framePassed = (int)double.Floor(Counter.Frame - Counter.LastFramesPassed) != (int)double.Floor(Counter.Frame);

            if (framePassed && Counter.FrameInt % 2 == 0
                && Counter.FrameInt < 8 && (Counter.Frame / 3) < _meteors.Count)
            {
                _audio.Play(Protos.Sound.AtkFire);
            }
        }

        public override void Update(float dt)
        {
            Counter.Update(dt);
            if (!_didAnything)
            {
                Finish();
                return;
            }

            var framesPassed = Counter.LastFramesPassed;

            for (var i = 0; i < _meteors.Count; i++)
            {
                var meteor = _meteors[i];

                if (meteor.Frame < 9)
                {
                    // falling
                    var dx = i % (_graphics.WindowPixelSize.X / 30);
                    var dy = i % (_graphics.WindowPixelSize.Y / 10);
                    meteor.Pos = new Vector2(
                        meteor.Pos.X - (16 + dx) * framesPassed,
                        meteor.Pos.Y + (24 + dy) * framesPassed);
                }
                meteor.Frame += framesPassed;
            }
        }
    }
    // <<<<<<<< shade2/screen.hsp:874 	swbreak ...
}
