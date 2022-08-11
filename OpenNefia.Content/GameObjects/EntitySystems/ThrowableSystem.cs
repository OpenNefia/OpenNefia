using OpenNefia.Content.Charas;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.GameObjects
{
    public class ThrowableSystem : EntitySystem
    {
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        public const string VerbTypeThrow = "Elona.Throw";

        public override void Initialize()
        {
            SubscribeComponent<ThrowableComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeComponent<ChipComponent, EntityThrownEventArgs>(ShowThrownChipRenderable, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ThrowableComponent, EntityThrownEventArgs>(HandleEntityThrown);
            SubscribeComponent<CharaComponent, HitByThrownEntityEventArgs>(HandleCharaHitByThrown);
        }

        private void HandleCharaHitByThrown(EntityUid uid, CharaComponent component, HitByThrownEntityEventArgs args)
        {
            args.WasHit = true;
        }

        private void ShowThrownChipRenderable(EntityUid target, ChipComponent targetChip, EntityThrownEventArgs args)
        {
            if (!EntityManager.TryGetComponent(args.Thrower, out SpatialComponent sourceSpatial))
                return;

            var animAttack = new RangedAttackMapDrawable(sourceSpatial.MapPosition, args.Coords, targetChip.ChipID, targetChip.Color);
            _mapDrawables.Enqueue(animAttack, sourceSpatial.MapPosition);

            var animBreaking = new BreakingFragmentsMapDrawable();
            _mapDrawables.Enqueue(animBreaking, args.Coords);
        }

        private void HandleGetVerbs(EntityUid target, ThrowableComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(VerbTypeThrow, "Throw Entity", () => Throw(args.Source, args.Target)));
        }

        private TurnResult Throw(EntityUid thrower, EntityUid item)
        {
            if (!EntityManager.TryGetComponent(thrower, out SpatialComponent sourceSpatial))
                return TurnResult.Failed;

            var posResult = _uiManager.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(new(sourceSpatial.MapPosition));
            if (!posResult.HasValue)
            {
                return TurnResult.Aborted;
            }

            if (!posResult.Value.CanSee)
            {
                _mes.Display("You can't see the location.");
                return TurnResult.Aborted;
            }

            if (!_stackSystem.TrySplit(item, 1, posResult.Value.Coords, out var split))
                return TurnResult.Failed;

            if (!ThrowEntity(thrower, split, posResult.Value.Coords))
                return TurnResult.Failed;

            return TurnResult.Succeeded;
        }

        public bool ThrowEntity(EntityUid thrower, EntityUid item, MapCoordinates coords)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map)
                || !EntityManager.IsAlive(thrower)
                || !EntityManager.IsAlive(item)
                || !Spatial(thrower).MapPosition.TryDistanceTiled(coords, out var dist))
                return false;

            _mes.Display(Loc.GetString("Elona.Throwable.Throws", ("entity", thrower), ("item", item)), entity: thrower);

            // Offset final position randomly based on Throwing skill
            if (dist * 4 > _rand.Next(_skills.Level(thrower, Protos.Skill.Throwing) + 10) + _skills.Level(thrower, Protos.Skill.Throwing) / 4
                || _rand.OneIn(10))
            {
                var newPos = coords.Position + _rand.NextVec2iInRadius(2);
                if (map.CanAccess(newPos))
                    coords = new(coords.MapId, newPos);
            }

            var ev = new EntityThrownEventArgs(thrower, coords);
            RaiseEvent(item, ev);
            return true;
        }

        private void HandleEntityThrown(EntityUid itemThrown, ThrowableComponent throwable, EntityThrownEventArgs args)
        {
            if (args.Handled)
                return;

            if (throwable.SplitAmount != null)
            {
                if (!_stackSystem.TrySplit(itemThrown, throwable.SplitAmount.Value, out var split))
                    return;
                itemThrown = split;
            }

            DoEntityThrown(itemThrown, args);
        }

        private void DoEntityThrown(EntityUid thrown,
            EntityThrownEventArgs args,
            SpatialComponent? sourceSpatial = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(args.Thrower, ref sourceSpatial))
                return;
            if (!Resolve(thrown, ref targetSpatial))
                return;
            if (sourceSpatial.MapPosition.MapId != args.Coords.MapId)
                return;

            args.Handled = true;

            // Place the entity on the map.
            targetSpatial.WorldPosition = args.Coords.Position;

            foreach (var onMapSpatial in _lookup.GetLiveEntitiesAtCoords(args.Coords))
            {
                if (onMapSpatial.Owner == thrown)
                    continue;

                var ev = new HitByThrownEntityEventArgs(args.Thrower, thrown, onMapSpatial.MapPosition);
                if (!Raise(onMapSpatial.Owner, ev) && EntityManager.IsAlive(thrown) && ev.WasHit)
                {
                    var ev2 = new ThrownEntityImpactedOtherEvent(args.Thrower, onMapSpatial.Owner, onMapSpatial.MapPosition);

                    if (!Raise(thrown, ev2))
                    {
                    }

                    return;
                }
                if (!EntityManager.IsAlive(thrown))
                {
                    return;
                }
            }

            var ev3 = new ThrownEntityImpactedGroundEvent(args.Thrower, args.Coords);

            if (!Raise(thrown, ev3))
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

        public bool OutDidImpact { get; set; } = true;

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
