using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Targetable;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Audio;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Effects.New.EffectAreas
{
    public sealed class VanillaEffectAreasSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly ITargetingSystem _targetings = default!;
        [Dependency] private readonly ITargetableSystem _targetable = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectAreaBoltComponent, ApplyEffectAreaEvent>(ApplyArea_Bolt);
            SubscribeComponent<EffectAreaBallComponent, ApplyEffectAreaEvent>(ApplyArea_Ball);
            SubscribeEntity<ApplyEffectAreaEvent>(ApplyAreaFallback, priority: EventPriorities.VeryLow + 100000);
            SubscribeComponent<EffectAreaAnimationComponent, GetEffectAnimationParamsEvent>(ApplyAreaAnimFallback, priority: EventPriorities.VeryLow + 100000);
        }


        /// <summary>
        /// If nothing handled an earlier <see cref="ApplyEffectAreaEvent"/>,
        /// then the effect will be applied directly to the target automatically.
        /// </summary>
        private void ApplyAreaFallback(EntityUid uid, ApplyEffectAreaEvent args)
        {
            if (args.Handled)
                return;

            var result = ApplyEffectDamage(uid, args.Source, args.Target, args.SourceCoords, args.TargetCoords, args.Args, 1, 0);

            args.CommonArgs.OutEffectWasObvious = result.EffectWasObvious;
            args.Handle(result.TurnResult);
        }

        private void ApplyAreaAnimFallback(EntityUid uid, EffectAreaAnimationComponent component, GetEffectAnimationParamsEvent args)
        {
            args.OutShowAnimation = component.ShowAnimation;
            if (component.Color != null)
                args.OutColor = component.Color.Value;
            if (component.Sound != null)
                args.OutSound = component.Sound.GetSound();
        }

        // TODO refactor into GetEffectMapDrawable?
        private bool TryGetEffectColorAndSound(EntityUid effect, EffectArgSet args, [NotNullWhen(true)] out EffectAnimationParams? result)
        {
            var ev = new GetEffectAnimationParamsEvent(args);
            RaiseEvent(effect, ev);
            if (ev.OutShowAnimation == false)
            {
                result = null;
                return false;
            }
            result = new(ev.OutColor, ev.OutSound);
            return true;
        }

        private void ApplyEffectTileDamage(EntityUid effect, EntityUid source, MapCoordinates coords, EffectArgSet args)
        {
            if (!coords.TryToEntity(_mapManager, out var entityCoords))
                return;
            ApplyEffectTileDamage(effect, source, entityCoords, args);
        }

        private void ApplyEffectTileDamage(EntityUid effect, EntityUid source, EntityCoordinates coords, EffectArgSet args)
        {
            var ev = new ApplyEffectTileDamageEvent(source, coords, args);
            RaiseEvent(effect, ev);
        }

        /// <summary>
        /// Gets a list of position offsets for use with a bolt.
        /// Because the bolt will pass *through* the target, offsets
        /// are used instead of physical coordinates, and the offsets
        /// list is wrapped around to extend the line of the bolt further
        /// than the target's position.
        /// </summary>
        /// <returns></returns>
        private bool TryGetBoltRoute(MapCoordinates from, MapCoordinates to, [NotNullWhen(true)] out IList<Vector2i>? offsets)
        {
            if (from.MapId != to.MapId || !TryMap(from, out var map))
            {
                offsets = null;
                return false;
            }

            offsets = new List<Vector2i>();

            if (from == to)
            {
                return false;
            }

            var lastPos = from.Position;

            foreach (var pos in PosHelpers.EnumerateLine(from.Position, to.Position))
            {
                if (map.CanSeeThrough(pos))
                {
                    offsets.Add(pos - lastPos);
                    lastPos = pos;
                }
                else
                    return false;
            }

            return offsets.Count > 0;
        }

        private const int BoltMaxTiles = 20;

        private void ApplyArea_Bolt(EntityUid uid, EffectAreaBoltComponent component, ApplyEffectAreaEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1697 	case skBolt ...
            if (!TryGetBoltRoute(args.SourceCoordsMap, args.TargetCoordsMap, out var offsets))
            {
                args.Handle(TurnResult.Failed);
                return;
            }

            var range = args.CommonArgs.TileRange;

            if (TryGetEffectColorAndSound(uid, args.Args, out var animParams))
            {
                var anim = new BoltMapDrawable(args.SourceCoordsMap, args.TargetCoordsMap, offsets, range, animParams.Color, animParams.Sound);
                _mapDrawables.Enqueue(anim, args.Source);
            }

            var curPos = args.SourceCoordsMap.Position;
            var map = args.SourceMap;

            var obvious = false;

            for (var i = 0; i < BoltMaxTiles; i++)
            {
                var offset = offsets[i % offsets.Count];
                curPos += offset;
                var curPosMap = map.AtPos(curPos);
                var curPosEntity = map.AtPosEntity(curPos);

                var isInFOV = map.IsInBounds(curPos) && map.CanSeeThrough(curPos) && _vis.IsInWindowFov(curPosMap);

                if ((component.IgnoreFOV || isInFOV) && i < range)
                {
                    ApplyEffectTileDamage(uid, args.Source, curPosMap, args.Args);

                    _targetable.TryGetTargetableEntity(curPosMap, out var innerTarget);
                    var result = ApplyEffectDamage(uid, args.Source, innerTarget?.Owner, args.SourceCoords, innerTarget?.Coordinates ?? curPosEntity, args.Args, offsets.Count, i);

                    obvious = obvious || result.EffectWasObvious;
                }
            }

            args.CommonArgs.OutEffectWasObvious = obvious;
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1717 	swbreak ...
        }

        private void ApplyArea_Ball(EntityUid uid, EffectAreaBallComponent component, ApplyEffectAreaEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1697 	case skBolt ...
            var map = args.SourceMap;
            var positions = PosHelpers.EnumerateBallPositions(args.SourceCoordsMap.Position, args.Args.TileRange, map.Bounds, component.IncludeOriginPos)
                .Select(p => map.AtPos(p))
                .ToList();

            if (positions.Count == 0)
            {
                args.Handle(TurnResult.Succeeded);
                return;
            }

            var range = args.CommonArgs.TileRange;

            if (TryGetEffectColorAndSound(uid, args.Args, out var animParams))
            {
                var anim = new BallMapDrawable(positions, animParams.Color, animParams.Sound);
                _mapDrawables.Enqueue(anim, args.Source);
            }

            var curPos = args.SourceCoordsMap.Position;

            var obvious = false;

            for (var i = 0; i < positions.Count; i++)
            {
                var coords = positions[i];

                ApplyEffectTileDamage(uid, args.Source, coords, args.Args);

                if (coords.TryToEntity(_mapManager, out var curPosEntity))
                {
                    _targetable.TryGetTargetableEntity(coords, out var innerTarget);
                    var result = ApplyEffectDamage(uid, args.Source, innerTarget?.Owner, args.SourceCoords, innerTarget?.Coordinates ?? curPosEntity, args.Args, positions.Count, i);

                    obvious = obvious || result.EffectWasObvious;
                }
            }

            args.CommonArgs.OutEffectWasObvious = obvious;
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1717 	swbreak ...
        }

        private EffectDamageResult ApplyEffectDamage(EntityUid uid, EntityUid source, EntityUid? innerTarget, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectArgSet args, int affectedTiles, int affectedTileIndex)
        {
            var ev = new ApplyEffectDamageEvent(source, innerTarget, sourceCoords, targetCoords, args, affectedTiles, affectedTileIndex);
            RaiseEvent(uid, ev);
            return new(ev.TurnResult, ev.OutEffectWasObvious);
        }
    }

    public sealed record class EffectDamageResult(TurnResult TurnResult, bool EffectWasObvious);

    public sealed record class EffectAnimationParams(Color Color, PrototypeId<SoundPrototype>? Sound);

    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectAnimationParamsEvent : EntityEventArgs
    {
        public GetEffectAnimationParamsEvent(EffectArgSet args)
        {
            Args = args;
        }

        public bool OutShowAnimation { get; set; } = true;
        public Color OutColor { get; set; } = Color.White;
        public PrototypeId<SoundPrototype>? OutSound { get; set; }
        public EffectArgSet Args { get; }
    }
}