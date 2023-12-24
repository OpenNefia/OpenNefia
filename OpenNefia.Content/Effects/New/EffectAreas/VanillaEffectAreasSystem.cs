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
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Effects.New;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Threading.Tasks.Sources;
using Love;
using Color = OpenNefia.Core.Maths.Color;

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
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IFormulaEngine _formulaEngine = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;

        public override void Initialize()
        {
            SubscribeEntity<ApplyEffectAreaEvent>(ApplyAreaFallback, priority: EventPriorities.VeryLow + 100000);
            SubscribeComponent<EffectAreaAnimationComponent, GetEffectAnimationParamsEvent>(ApplyAreaAnimFallback, priority: EventPriorities.VeryLow + 100000);
            SubscribeComponent<EffectAreaMessageComponent, ApplyEffectAreaEvent>(ShowAreaMessage, priority: EventPriorities.VeryHigh + 100000);

            SubscribeComponent<EffectAreaArrowComponent, ApplyEffectAreaEvent>(ApplyArea_Arrow);
            SubscribeComponent<EffectAreaBoltComponent, ApplyEffectAreaEvent>(ApplyArea_Bolt);
            SubscribeComponent<EffectAreaBallComponent, ApplyEffectAreaEvent>(ApplyArea_Ball);
            SubscribeComponent<EffectAreaBreathComponent, ApplyEffectAreaEvent>(ApplyArea_Breath);
            SubscribeComponent<EffectAreaWebComponent, ApplyEffectAreaEvent>(ApplyArea_Web);
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

            if (!result.EventWasHandled)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

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

        private void ShowAreaMessage(EntityUid uid, EffectAreaMessageComponent component, ApplyEffectAreaEvent args)
        {
            _mes.Display(Loc.GetString(component.MessageKey, ("source", args.Source), ("target", args.Target), ("sourceItem", args.CommonArgs.SourceItem), ("targetItem", args.CommonArgs.TargetItem)));
            var soundId = component.Sound?.GetSound();
            if (soundId != null)
                _audio.Play(soundId.Value, args.TargetCoordsMap);
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

        private void ApplyArea_Arrow(EntityUid uid, EffectAreaArrowComponent component, ApplyEffectAreaEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1810 	case skArrow ...
            if (TryGetEffectColorAndSound(uid, args.Args, out var animParams))
            {
                var anim = new RangedAttackMapDrawable(args.SourceCoordsMap, args.TargetCoordsMap, Protos.Chip.ItemProjectileMagicArrow, animParams.Color, Protos.Sound.Arrow1);
                _mapDrawables.Enqueue(anim, args.Source);
            }

            var result = ApplyEffectDamage(uid, args.Source, args.Target, args.SourceCoords, null, args.Args, 1, 0);

            if (!result.EventWasHandled)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            args.CommonArgs.OutEffectWasObvious = result.EffectWasObvious;
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1828 	swbreak ...
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
            var didSomething = false;

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
                    didSomething = didSomething || result.EventWasHandled;
                }
            }

            if (!didSomething)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
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
                .Where(p => map.HasLineOfSight(args.SourceCoordsMap.Position, p))
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
            var didSomething = false;

            for (var i = 0; i < positions.Count; i++)
            {
                var coords = positions[i];

                ApplyEffectTileDamage(uid, args.Source, coords, args.Args);

                if (coords.TryToEntity(_mapManager, out var curPosEntity))
                {
                    _targetable.TryGetTargetableEntity(coords, out var innerTarget);
                    var result = ApplyEffectDamage(uid, args.Source, innerTarget?.Owner, args.SourceCoords, innerTarget?.Coordinates ?? curPosEntity, args.Args, positions.Count, i);

                    obvious = obvious || result.EffectWasObvious;
                    didSomething = didSomething || result.EventWasHandled;
                }
            }

            if (!didSomething)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            args.CommonArgs.OutEffectWasObvious = obvious;
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1717 	swbreak ...
        }

        public IEnumerable<Vector2i> EnumerateBreathPositions(Vector2i from, Vector2i to, int tileRange, UIBox2i bounds)
        {
            var breathWidth = 1;
            var count = 0;
            var seen = new HashSet<Vector2i>();

            foreach (var pos in PosHelpers.EnumerateLine(from, to, includeStartPos: true).Take(tileRange))
            {
                if (count < 6)
                {
                    if (count % 3 == 1)
                    {
                        breathWidth += 2;
                    }
                }
                else
                {
                    breathWidth = int.Max(breathWidth - 2, 3);
                }

                count++;
                if (count > tileRange)
                    break;

                for (var j = 0; j < breathWidth; j++)
                {
                    var ty = j - breathWidth / 2 + pos.Y;
                    for (var i = 0; i < breathWidth; i++)
                    {
                        var tx = i - breathWidth / 2 + pos.X;

                        var rpos = new Vector2i(tx, ty);
                        if (!seen.Contains(rpos))
                        {
                            yield return rpos;
                            seen.Add(rpos);
                        }

                        // 1.22 behavior
                        // if (seen.Count >= 100)
                        //     yield break;
                    }
                }
            }
        }

        private void ApplyArea_Breath(EntityUid uid, EffectAreaBreathComponent component, ApplyEffectAreaEvent args)
        {
            if (args.Handled)
                return;

            var map = args.SourceMap;
            if (!map.HasLineOfSight(args.SourceCoordsMap, args.TargetCoordsMap) || !map.IsInWindowFov(args.SourceCoordsMap))
            {
                args.Handle(TurnResult.Failed);
                return;
            }

            string breathName;
            if (TryComp<EffectDamageElementalComponent>(uid, out var effElem) && effElem.Element != null)
            {
                breathName = Loc.GetPrototypeString(effElem.Element.Value, "Name");
                breathName = Loc.GetString("Elona.Magic.Message.Breath.Named", ("breathName", breathName));
            }
            else
            {
                breathName = Loc.GetString(component.BreathNameKey);
            }
            _mes.Display(Loc.GetString("Elona.Magic.Message.Breath.Bellows", ("source", args.Source), ("breathName", breathName)), entity: args.Source);

            var positions = EnumerateBreathPositions(args.SourceCoordsMap.Position, args.TargetCoordsMap.Position, args.Args.TileRange, args.TargetMap.Bounds)
                .Where(p => map.HasLineOfSight(args.SourceCoordsMap.Position, p))
                .Select(p => map.AtPos(p))
                .ToList();

            if (TryGetEffectColorAndSound(uid, args.Args, out var animParams))
            {
                var anim = new BreathMapDrawable(positions, args.SourceCoordsMap, args.TargetCoordsMap, animParams.Color, animParams.Sound);
                _mapDrawables.Enqueue(anim, args.Source);
            }

            var obvious = false;
            var didSomething = false;

            for (var i = 0; i < positions.Count; i++)
            {
                var curPosMap = positions[i];
                if (component.IncludeOriginPos || curPosMap != args.SourceCoordsMap)
                {
                    ApplyEffectTileDamage(uid, args.Source, curPosMap, args.Args);

                    if (curPosMap.TryToEntity(_mapManager, out var curPosEntity))
                    {
                        _targetable.TryGetTargetableEntity(curPosMap, out var innerTarget);
                        var result = ApplyEffectDamage(uid, args.Source, innerTarget?.Owner, args.SourceCoords, innerTarget?.Coordinates ?? curPosEntity, args.Args, positions.Count, i);

                        obvious = obvious || result.EffectWasObvious;
                        didSomething = didSomething || result.EventWasHandled;
                    }
                }
            }

            if (!didSomething)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            args.CommonArgs.OutEffectWasObvious = obvious;
            args.Handle(TurnResult.Succeeded);
        }

        private IList<MapCoordinates> GetWebPositions(MapCoordinates origin, int spread, int attempts)
        {
            var map = GetMap(origin);
            var positions = new List<MapCoordinates>();

            while (attempts > 0)
            {
                var pos = origin.Position + _rand.NextVec2iInVec(spread, spread) - _rand.NextVec2iInVec(spread, spread);

                var canAccess = map.IsFloor(pos) && double.Floor((pos - origin.Position).Length) < spread;

                if (canAccess)
                {
                    positions.Add(map.AtPos(pos));
                    attempts--;
                }
                else if (_rand.OneIn(2))
                {
                    attempts--;
                }
            }

            return positions;
        }

        private void ApplyArea_Web(EntityUid uid, EffectAreaWebComponent component, ApplyEffectAreaEvent args)
        {
            if (args.Handled)
                return;

            var vars = _newEffects.GetEffectDamageFormulaArgs(uid, args.Source, args.Target, args.SourceCoords, args.TargetCoords, args.Args);
            var attempts = (int)_formulaEngine.Calculate(component.TileCount, vars, 10);
            var spread = (int)_formulaEngine.Calculate(component.Spread, vars, 3);

            var positions = GetWebPositions(args.TargetCoordsMap, spread, attempts);

            var obvious = false;
            var didSomething = false;
            var i = 0;

            foreach (var curPosMap in positions)
            {
                ApplyEffectTileDamage(uid, args.Source, curPosMap, args.Args);

                if (curPosMap.TryToEntity(_mapManager, out var curPosEntity))
                {
                    _targetable.TryGetTargetableEntity(curPosMap, out var innerTarget);
                    var result = ApplyEffectDamage(uid, args.Source, innerTarget?.Owner, args.SourceCoords, innerTarget?.Coordinates ?? curPosEntity, args.Args, positions.Count, i);

                    obvious = obvious || result.EffectWasObvious;
                    didSomething = didSomething || result.EventWasHandled;
                }
            }

            if (!didSomething)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            args.CommonArgs.OutEffectWasObvious = obvious;
            args.Handle(TurnResult.Succeeded);
        }

        private EffectDamageResult ApplyEffectDamage(EntityUid uid, EntityUid source, EntityUid? innerTarget, EntityCoordinates sourceCoords, EntityCoordinates? targetCoords, EffectArgSet args, int affectedTiles, int affectedTileIndex)
        {
            if (targetCoords == null)
            {
                if (IsAlive(innerTarget))
                    targetCoords = Spatial(innerTarget.Value).Coordinates;
            }
            targetCoords ??= sourceCoords;

            var ev = new ApplyEffectDamageEvent(source, innerTarget, sourceCoords, targetCoords.Value, args, affectedTiles, affectedTileIndex);
            RaiseEvent(uid, ev);
            return new(ev.TurnResult, ev.OutEffectWasObvious, ev.Handled);
        }
    }

    public sealed record class EffectDamageResult(TurnResult TurnResult, bool EffectWasObvious, bool EventWasHandled);

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