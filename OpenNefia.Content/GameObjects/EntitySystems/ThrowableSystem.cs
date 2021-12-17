using OpenNefia.Content.Logic;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.GameObjects
{
    public class ThrowableSystem : EntitySystem
    {
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;
        [Dependency] private readonly IUiLayerManager _uiLayers = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public const string VerbIDThrow = "Elona.Throw";

        public override void Initialize()
        {
            SubscribeLocalEvent<ThrowableComponent, GetVerbsEventArgs>(HandleGetVerbs, nameof(HandleGetVerbs));
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb, nameof(HandleExecuteVerb));
            SubscribeLocalEvent<ChipComponent, EntityThrownEventArgs>(ShowThrownChipRenderable, nameof(ShowThrownChipRenderable),
                before: new[] { new SubId(typeof(ThrowableSystem), nameof(HandleEntityThrown)) });
            SubscribeLocalEvent<ThrowableComponent, EntityThrownEventArgs>(HandleEntityThrown, nameof(HandleEntityThrown));
            SubscribeLocalEvent<CharaComponent, HitByThrownEntityEventArgs>(HandleCharaHitByThrown, nameof(HandleCharaHitByThrown));
        }

        private void HandleCharaHitByThrown(EntityUid uid, CharaComponent component, HitByThrownEntityEventArgs args)
        {
            args.WasHit = true;
        }

        private void ShowThrownChipRenderable(EntityUid target, ChipComponent targetChip, EntityThrownEventArgs args)
        {
            if (!EntityManager.TryGetComponent(args.Thrower, out SpatialComponent sourceSpatial))
                return;

            var drawable = new RangedAttackMapDrawable(sourceSpatial.MapPosition, args.Coords, targetChip.ChipID, targetChip.Color);
            _mapDrawables.Enqueue(drawable, sourceSpatial.MapPosition);
        }

        private void HandleGetVerbs(EntityUid target, ThrowableComponent component, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(VerbIDThrow));
        }

        private void HandleExecuteVerb(ExecuteVerbEventArgs args)
        {
            switch (args.Verb.ID)
            {
                case VerbIDThrow:
                    ExecuteVerbThrow(args);
                    break;
            }
        }

        private void ExecuteVerbThrow(ExecuteVerbEventArgs args)
        {
            var source = args.Source;

            if (args.Handled || !EntityManager.TryGetEntity(source, out var sourceEntity))
                return;

            args.Handled = true;

            if (!_mapManager.TryGetMap(sourceEntity.Spatial.MapID, out var map))
                return;

            var prompt = new PositionPrompt(sourceEntity);
            var posResult = _uiLayers.Query(prompt);
            if (!posResult.HasValue)
            {
                args.TurnResult = TurnResult.Aborted;
                return;
            }

            if (!posResult.Value.CanSee)
            {
                Mes.Display("You can't see the location.");
                args.TurnResult = TurnResult.Failed;
                return;
            }

            // TODO stacking

            if (ThrowEntity(source, args.Target, posResult.Value.Coords))
            {
                args.TurnResult = TurnResult.Succeeded;
            }
        }

        public bool ThrowEntity(EntityUid source, EntityUid throwing, MapCoordinates coords)
        {
            if (!_mapManager.MapExists(coords.MapId) 
                || !EntityManager.IsAlive(source) 
                || !EntityManager.IsAlive(throwing))
                return false;

            var ev = new EntityThrownEventArgs(source, coords);
            RaiseLocalEvent(throwing, ev);
            return true;
        }

        private void HandleEntityThrown(EntityUid target, ThrowableComponent throwable, EntityThrownEventArgs args)
        {
            if (args.Handled)
                return;

            DoEntityThrown(target, args);
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
            if (sourceSpatial.MapPosition.MapId != args.Coords.MapId)
                return;

            args.Handled = true;

            foreach (var entity in _lookup.GetLiveEntitiesAtPos(args.Coords))
            {
                var ev = new HitByThrownEntityEventArgs(args.Thrower, target, entity.Spatial.MapPosition);
                RaiseLocalEvent(entity.Uid, ev);
                if (ev.WasHit)
                {
                    var ev2 = new ThrownEntityImpactedOtherEvent(args.Thrower, entity.Uid, entity.Spatial.MapPosition);
                    RaiseLocalEvent(target, ev2);

                    if (!ev2.Handled && EntityManager.IsAlive(target))
                    {
                        // Place the entity on the map.
                        targetSpatial.WorldPosition = args.Coords.Position;
                    }

                    return;
                }
            }

            var ev3 = new ThrownEntityImpactedGroundEvent(args.Thrower, args.Coords);
            RaiseLocalEvent(target, ev3);

            if (!ev3.Handled && EntityManager.IsAlive(target))
            {
                // Place the entity on the map.
                targetSpatial.WorldPosition = args.Coords.Position;
            }
        }
    }

    public class HitByThrownEntityEventArgs : HandledEntityEventArgs
    {
        public readonly EntityUid Thrower;
        public readonly EntityUid Thrown;
        public readonly MapCoordinates Coords;

        public bool WasHit = false;

        public HitByThrownEntityEventArgs(EntityUid thrower, EntityUid thrown, MapCoordinates coords)
        {
            this.Thrower = thrower;
            this.Thrown = thrown;
            this.Coords = coords;
        }
    }

    public class ThrownEntityImpactedOtherEvent : HandledEntityEventArgs
    {
        public readonly EntityUid Thrower;
        public readonly EntityUid ImpactedWith;
        public readonly MapCoordinates Coords;

        public ThrownEntityImpactedOtherEvent(EntityUid thrower, EntityUid impactedWith, MapCoordinates coords)
        {
            this.Thrower = thrower;
            this.ImpactedWith = impactedWith;
            this.Coords = coords;
        }
    }

    public class ThrownEntityImpactedGroundEvent : HandledEntityEventArgs
    {
        public readonly EntityUid Thrower;
        public readonly MapCoordinates Coords;

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
