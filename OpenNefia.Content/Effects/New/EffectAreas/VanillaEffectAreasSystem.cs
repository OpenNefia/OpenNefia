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

        public override void Initialize()
        {
            SubscribeComponent<EffectAreaBoltComponent, ApplyEffectAreaEvent>(ApplyArea_Bolt);
        }

        private const int RANGE_BOLT = 6;
        private const int RANGE_BALL = 2;
        private const int RANGE_BREATH = 5;

        private Color GetEffectColor(EntityUid effect, EffectArgSet args)
        {
            var ev = new GetEffectColorEvent(args);
            RaiseEvent(effect, ev);
            return ev.OutColor;
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

            foreach (var pos in PosHelpers.EnumerateLine(from.Position, to.Position))
            {
                if (map.CanSeeThrough(pos))
                    offsets.Add(pos - from.Position);
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

            // TODO anim
            var color = GetEffectColor(uid, args.Args);

            var curPos = args.SourceCoordsMap.Position;
            var map = args.Map;

            for (var i = 0; i < BoltMaxTiles; i++)
            {
                var offset = offsets[i % offsets.Count];
                curPos += offset;
                var curPosMap = map.AtPos(curPos);

                if (i < offsets.Count 
                    || (map.IsInBounds(curPos)
                        && map.CanSeeThrough(curPos) 
                        && _vis.IsInWindowFov(curPosMap)))
                {
                    ApplyEffectTileDamage(uid, args.Source, curPosMap, args.Args);

                    if (_targetable.TryGetTargetableEntity(curPosMap, out var innerTarget))
                    {

                    }
                }
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1717 	swbreak ...
        }
    }

    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectColorEvent : EntityEventArgs
    {
        public GetEffectColorEvent(EffectArgSet args)
        {
            Args = args;
        }

        public Color OutColor { get; set; } = Color.White;
        public EffectArgSet Args { get; }
    }
}