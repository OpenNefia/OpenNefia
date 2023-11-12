using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace OpenNefia.Content.Combat
{
    public sealed class MeleeAttackMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;

        private bool _isCritical;
        private bool _hasAttackAnim;
        private IAssetInstance _assetAttackAnim;
        private IAssetInstance _assetParticle;
        private IAssetInstance _assetCritical;
        private List<Vector2i> _points = new();

        private FrameCounter _counter;

        public MeleeAttackMapDrawable(PrototypeId<AssetPrototype> assetParticle, PrototypeId<AssetPrototype>? assetAttackAnim, int damagePercent, bool isCritical)
        {
            // TODO
            IoCManager.InjectDependencies(this);

            _assetParticle = Assets.Get(assetParticle);
            _hasAttackAnim = assetAttackAnim != null;
            _assetAttackAnim = Assets.Get(assetAttackAnim ?? Protos.Asset.SwarmEffect);
            _assetCritical = Assets.Get(Protos.Asset.AnimCritical);
            _isCritical = isCritical;

            var pointCount = Math.Min(damagePercent / 4 + 1, 20);
            for (int i = 0; i < pointCount; i++)
            {
                _points.Add(new Vector2i(_rand.Next(24) - 12, _rand.Next(8)));
            }

            var waitSecs = IoCManager.Resolve<IConfigurationManager>().GetCVar(CCVars.AnimeAnimationWait) / 5;

            var frames = 4u;
            if (_hasAttackAnim)
                frames = _assetAttackAnim.CountX;
            if (isCritical)
                frames = Math.Max(frames, _assetCritical.CountX);

            _counter = new FrameCounter(waitSecs, (int)frames);
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

            var frame2 = _counter.Frame * 2f;

            if (_isCritical && _counter.FrameInt < (int)_assetCritical.CountX)
                _assetCritical.DrawRegionUnscaled(_counter.FrameInt.ToString(), PixelX - 24 * _coords.TileScale - ScreenOffset.X, PixelY - 32 * _coords.TileScale - ScreenOffset.Y, 96 * _coords.TileScale, 96 * _coords.TileScale); // TODO fix...

            for (var i = 0; i < _points.Count; i++)
            {
                var point = _points[i];
                var dx = (float)(point.X + _coords.TileSize.X / 2);
                if (point.X < 4)
                {
                    dx -= frame2;
                    if (_counter.FrameInt % 2 == 0)
                        dx -= frame2;
                }
                if (point.X > -4)
                {
                    dx += frame2;
                    if (_counter.FrameInt % 2 == 0)
                    {
                        dx += frame2;
                    }
                }

                var dy = point.Y + frame2 * (frame2 / 3);
                _assetParticle.DrawUnscaled(PixelX - ScreenOffset.X + dx * _coords.TileScale, PixelY - ScreenOffset.Y + dy * _coords.TileScale, 6 * _coords.TileScale, 6 * _coords.TileScale, centered: true, rotationRads: 0.4f * _counter.Frame);
            }

            if (_counter.FrameInt < _assetAttackAnim.CountX)
            {
                if (_hasAttackAnim)
                {
                    _assetAttackAnim.DrawRegionUnscaled(_counter.FrameInt.ToString(), PixelX - ScreenOffset.X, PixelY - ScreenOffset.Y, _coords.TileSizeScaled.X, _coords.TileSizeScaled.Y);
                }
                else
                {
                    var firstPoint = _points.FirstOrDefault();
                    var size = (_counter.FrameInt * 10 + _points.Count) * _coords.TileScale;
                    _assetAttackAnim.DrawRegionUnscaled(
                        _counter.FrameInt.ToString(),
                        PixelX - ScreenOffset.X + firstPoint.X * _coords.TileScale + _coords.TileSizeScaled.X / 2,
                        PixelY - ScreenOffset.Y + firstPoint.Y * _coords.TileScale + 10, 
                        size,
                        size,
                        centered: true,
                        rotationRads: 0.5f + _counter.Frame * 0.8f);
                }
            }
            // <<<<<<<< shade2/screen.hsp:746 	loop ...
        }
    }
}