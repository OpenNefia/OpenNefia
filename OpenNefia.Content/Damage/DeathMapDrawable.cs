using OpenNefia.Content.Resists;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Damage
{
    internal class DeathMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;
        [Dependency] protected readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private bool _isCritical;
        private bool _hasAttackAnim;
        private IAssetInstance _fragmentsAnim;
        private IAssetInstance? _elementAnim;
        private int _elementAnimDy = -16;
        private PrototypeId<ElementPrototype>? _elementID;
        private List<Vector2i> _points = new();

        private FrameCounter _counter;

        public DeathMapDrawable(PrototypeId<AssetPrototype> deathFragments, PrototypeId<ElementPrototype>? elementID = null)
        {
            // TODO
            IoCManager.InjectDependencies(this);

            _fragmentsAnim = Assets.Get(deathFragments);
            if (elementID != null)
            {
                var element = _protos.Index(elementID.Value);
                if (element.DeathAnim != null)
                {
                    _elementAnim = Assets.Get(element.DeathAnim.Value);
                    _elementAnimDy = element.DeathAnimDy;
                }
            }

            var pointCount = 20;
            for (int i = 0; i < pointCount; i++)
            {
                _points.Add(new Vector2i(_rand.Next(_coords.TileSizeScaled.X) - (_coords.TileSizeScaled.X / 2), _coords.TileSizeScaled.Y / 2));
            }

            var waitSecs = 0.15f;
            if (_elementAnim != null)
                waitSecs += 0.20f;

            _counter = new FrameCounter(waitSecs * _config.GetCVar(CCVars.AnimeAnimationWait), 6);
        }

        public override void Update(float dt)
        {
            _counter.Update(dt);
            if (_counter.IsFinished)
                Finish();
        }

        public override void Draw()
        {
            // >>>>>>>> shade2 / screen.hsp:701   case aniNormalAttack...
            Love.Graphics.SetColor(Color.White);

            var frame2 = _counter.FrameInt * 2;

            if (_elementAnim != null && _counter.FrameInt < _elementAnim.CountX)
                _elementAnim.DrawRegion(_coords.TileScale, _counter.FrameInt.ToString(), X - ScreenOffset.X - _coords.TileSize.X / 2, Y - ScreenOffset.Y - (3 * _coords.TileSize.Y / 4) + _elementAnimDy);

            if (_counter.FrameInt >= _fragmentsAnim.CountX)
                return;

            for (var i = 0; i < _points.Count; i++)
            {
                var point = _points[i];
                var dx = 0;

                if (point.X < 3)
                {
                    dx -= frame2;
                    if (_counter.FrameInt % 2 == 0)
                        dx -= frame2;
                }
                if (point.X > -3)
                {
                    dx += frame2;
                    if (_counter.FrameInt % 2 == 0)
                    {
                        dx += frame2;
                    }
                }

                _fragmentsAnim.DrawRegion(_coords.TileScale,
                   _counter.FrameInt.ToString(),
                   X + _coords.TileSize.X / 2 + point.X + dx,
                   Y + frame2 * frame2 / 2 - 12 + i + point.Y,
                   (_coords.TileSize.X / 2) - frame2 * 2,
                   (_coords.TileSize.Y / 2) - frame2 * 2,
                   true,
                   0.2f * i);
            }
            // <<<<<<<< shade2/screen.hsp:746 	loop ...
        }
    }
}