using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public class ThrowableSystem : EntitySystem
    {
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;
        [Dependency] private readonly IUiLayerManager _uiLayers = default!;
        [Dependency] private readonly IMapManager _map = default!;

        public const string VerbIDThrow = "Elona.Throw";

        public override void Initialize()
        {
            SubscribeLocalEvent<ThrowableComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb);
            SubscribeLocalEvent<ThrowableComponent, EntityThrownEventArgs>(HandleEntityThrown);
        }

        private void HandleGetVerbs(EntityUid target, ThrowableComponent component, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(VerbIDThrow));
        }

        private void HandleExecuteVerb(ExecuteVerbEventArgs args)
        {
            if (args.Verb.ID != VerbIDThrow)
                return;

            var source = args.Source;

            if (args.Handled || !EntityManager.TryGetEntity(source, out var sourceEntity))
                return;

            var map = sourceEntity.Spatial.Map;
            if (map == null)
                return;

            args.Handled = true;

            var prompt = new PositionPrompt(sourceEntity);
            var posResult = _uiLayers.Query(prompt);
            if (!posResult.HasValue)
            {
                args.TurnResult = TurnResult.Aborted;
                return;
            }

            if (ThrowEntity(source, args.Target, posResult.Value.Coords))
            {
                args.TurnResult = TurnResult.Succeeded;
            }
        }

        public bool ThrowEntity(EntityUid source, EntityUid throwing, MapCoordinates coords)
        {
            var map = coords.Map;
            if (map == null || !EntityManager.IsAlive(source) || !EntityManager.IsAlive(throwing))
                return false;

            var ev = new EntityThrownEventArgs(source, coords);
            RaiseLocalEvent(throwing, ev);
            return true;
        }

        private void HandleEntityThrown(EntityUid uid, ThrowableComponent throwable, EntityThrownEventArgs args)
        {
            if (args.Handled)
                return;

            DoEntityThrown(uid, args);
        }

        private void DoEntityThrown(EntityUid target, 
            EntityThrownEventArgs args,
            SpatialComponent? sourceSpatial = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(args.Thrower, ref sourceSpatial))
                return;
            if (!Resolve(target, ref targetSpatial))
                return;
            if (sourceSpatial.Coords.Map != args.Coords.Map)
                return;

            args.Handled = true;

            if (EntityManager.TryGetComponent(target, out ChipComponent? targetChip))
            {
                var drawable = new RangedAttackMapDrawable(sourceSpatial.Coords, args.Coords, targetChip.ID, targetChip.Color);
                _mapDrawables.Enqueue(drawable, sourceSpatial.Coords);
            }

            foreach (var entity in _map.GetEntities(args.Coords))
            {
                if (entity.Spatial.IsSolid)
                {
                    var ev = new HitByThrownEntityEventArgs(args.Thrower, target);
                    RaiseLocalEvent(entity.Uid, ev);
                    if (ev.WasHit)
                    {

                        var ev2 = new ThrownEntityImpactedOtherEvent(args.Thrower, entity.Uid);
                        RaiseLocalEvent(target, ev2);
                        return;
                    }
                }
            }

            var ev3 = new ThrownEntityImpactedGroundEvent(args.Thrower, args.Coords);
            RaiseLocalEvent(target, ev3);

            if (!ev3.Handled && EntityManager.IsAlive(target))
            {
                // Place the entity on the map.
                targetSpatial.Pos = args.Coords.Position;
            }
        }
    }

    public class HitByThrownEntityEventArgs : HandledEntityEventArgs
    {
        public readonly EntityUid Thrower;
        public readonly EntityUid Thrown;

        public bool WasHit = true;

        public HitByThrownEntityEventArgs(EntityUid thrower, EntityUid thrown)
        {
            this.Thrower = thrower;
            this.Thrown = thrown;
        }
    }

    public class ThrownEntityImpactedOtherEvent : HandledEntityEventArgs
    {
        private EntityUid Thrower;
        private EntityUid ImpactedWith;

        public ThrownEntityImpactedOtherEvent(EntityUid thrower, EntityUid impactedWith)
        {
            this.Thrower = thrower;
            this.ImpactedWith = impactedWith;
        }
    }

    public class ThrownEntityImpactedGroundEvent : HandledEntityEventArgs
    {
        private EntityUid Thrower;
        private MapCoordinates Coords;

        public bool DidImpact = true;

        public ThrownEntityImpactedGroundEvent(EntityUid thrower, MapCoordinates coords)
        {
            this.Thrower = thrower;
            this.Coords = coords;
        }
    }

    public class EntityThrownEventArgs : HandledEntityEventArgs
    {
        public readonly EntityUid Thrower;

        public readonly MapCoordinates Coords;

        public EntityThrownEventArgs(EntityUid thrower, MapCoordinates coords)
        {
            Thrower = thrower;
            Coords = coords;
        }
    }
}
