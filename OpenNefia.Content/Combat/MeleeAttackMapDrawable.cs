﻿using OpenNefia.Content.Prototypes;
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

        private bool _breaksIntoDebris;
        private int _damagePercent;
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
            _hasAttackAnim = _assetAttackAnim != null;
            _assetAttackAnim = Assets.Get(assetAttackAnim ?? Protos.Asset.SwarmEffect);
            _assetCritical = Assets.Get(Protos.Asset.AnimCritical);
            _damagePercent = damagePercent;
            _isCritical = isCritical;

            var pointCount = Math.Min(damagePercent / 4 + 1, 20);
            for (int i = 0; i < pointCount; i++)
            {
                _points.Add(new Vector2i(_rand.Next(24) - 12, _rand.Next(8)));
            }

            var waitSecs = IoCManager.Resolve<IConfigurationManager>().GetCVar(CCVars.AnimeAnimationWait) * 10;

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
            if (_isCritical)
                _assetCritical.DrawRegionUnscaled(_counter.FrameInt.ToString(), PixelX - 24, PixelY - 32);

            for (var i = 0; i < _points.Count; i++)
            {
                var frame2 = _counter.FrameInt + 2;
                var point = _points[i];
                var dx = point.X + _coords.TileSize.X / 2;
                if (point.X < 4)
                {
                    dx -= frame2;
                    if (_counter.FrameInt % 2 == 0)
                        dx -= frame2;
                }
                if (point.X < -4)
                {
                    dx += frame2;
                    if (_counter.FrameInt % 2 == 0)
                    {
                        dx += frame2;
                    }
                }

                var dy = point.Y + frame2 * (frame2 / 3);
                _assetParticle.DrawUnscaled(PixelX + dx, PixelY + dy, 6, 6, centered: true, rotationRads: 0.4f * _counter.FrameInt);
            }

            if (_hasAttackAnim)
            {
                if (_counter.FrameInt < _assetAttackAnim.CountX)
                {
                    _assetAttackAnim.DrawRegionUnscaled(_counter.FrameInt.ToString(), PixelX, PixelY);
                }
            }
            else
            {
                var firstPoint = _points.FirstOrDefault();
                var size = _counter.FrameInt * 10 + _points.Count;
                _assetAttackAnim.DrawUnscaled(PixelX + firstPoint.X + _coords.TileSize.X / 2, PixelY + firstPoint.Y + 10, size, size, centered: true, rotationRads: 0.5f + _counter.Frame * 0.8f);
            }
        }
    }
}