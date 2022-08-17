using OpenNefia.Analyzers;
using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.EntityGen
{
    /// <summary>
    /// Wraps <see cref="IEntityManager"/>'s spawning functionality with additional event
    /// hooks for initializing entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is necessary since <see cref="IEntityManager.SpawnEntity"/> is general-purpose
    /// and gets used for instantiating entities loaded from a save game. On the contrary,
    /// some events should only be run the very first time the entity is spawned, and not when
    /// they're loaded from a save.
    /// </para>
    /// </remarks>
    public interface IEntityGen : IEntitySystem
    {
        void FireGeneratingEvents(EntityUid entity, EntityCoordinates? coords = null, EntityCoordinates? actualCoordinates = null, int? originalCount = null, EntityGenArgSet? args = null, IContainer? container = null);
        EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates, int? amount = null, EntityGenArgSet? args = null);
        EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates, int? amount = null, EntityGenArgSet? args = null);
        EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityUid entity, int? amount = null, EntityGenArgSet? args = null);
        EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, IContainer container, int? amount = null, EntityGenArgSet? args = null);
        EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, IMap map, int? amount = null, EntityGenArgSet? args = null);
    }

    public class EntityGenSystem : EntitySystem, IEntityGen
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IMapPlacement _placement = default!;
        [Dependency] private readonly IContainerSystem _containers = default!;

        public override void Initialize()
        {
            _mapLoader.OnBlueprintEntityStartup += HandleBlueprintEntityStartup;

            SubscribeEntity<AfterEntityClonedEvent>(HandleClone, priority: EventPriorities.VeryLow);
        }

        public void FireGeneratingEvents(EntityUid entity, EntityCoordinates? coords = null, EntityCoordinates? actualCoordinates = null, int? originalCount = null, EntityGenArgSet? args = null, IContainer? container = null)
        {
            coords ??= Spatial(entity).Coordinates;
            actualCoordinates ??= coords;
            args ??= EntityGenArgSet.Make();
            var ev1 = new EntityBeingGeneratedEvent(coords.Value, actualCoordinates.Value, originalCount, args, container);
            RaiseEvent(entity, ref ev1);

            // TODO: Check if generated has already been fired for this entity.
            var ev2 = new EntityGeneratedEvent(coords.Value, actualCoordinates.Value, originalCount, container);
            RaiseEvent(entity, ref ev2);
        }

        /// <summary>
        /// Runs entity generation events for entities loaded from blueprints.
        /// </summary>
        private void HandleBlueprintEntityStartup(EntityUid entity)
        {
            IContainer? container = null;
            if (_containers.TryGetContainingContainer(entity, out var found))
                container = found;
            FireGeneratingEvents(entity, container: container);
        }

        /// <summary>
        /// Runs entity generation events for all cloned entities.
        /// </summary>
        private void HandleClone(EntityUid entity, AfterEntityClonedEvent args)
        {
            // NOTE: Not running EntityBeingGeneratedEvent here, as that would redo random
            // initializations and things.
            IContainer? container = null;
            if (_containers.TryGetContainingContainer(args.ClonedFrom, out var found))
                container = found;
            var ev = new EntityGeneratedEvent(Spatial(entity).Coordinates, Spatial(entity).Coordinates, _stacks.GetCount(args.ClonedFrom), container);
            RaiseEvent(entity, ref ev);
        }

        private EntityUid? SpawnEntityInternal(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates, int? count, EntityGenArgSet? args, IContainer? container)
        {
            if (!coordinates.IsValid(EntityManager))
                return null;

            var ent = EntityManager.SpawnEntity(protoId, new MapCoordinates(MapId.Global, Vector2i.Zero));
            var originalCount = count;

            EntityGenCommonArgs? commonArgs = null;
            if (count == null && args != null && args.TryGet<EntityGenCommonArgs>(out commonArgs))
                count = commonArgs.Amount;

            if (EntityManager.HasComponent<StackComponent>(ent))
                _stacks.SetCount(ent, Math.Max(count ?? 1, 1));
            else if (count != null)
                Logger.WarningS("entity.gen", $"Passed count {count} to generate entity {protoId}, but entity did not have a {nameof(StackComponent)}.");

            var searchType = GetSearchType(protoId);
            var spatial = EntityManager.GetComponent<SpatialComponent>(ent);
            var actualCoordinates = coordinates;

            switch (searchType)
            {
                case PositionSearchType.Chara:
                    _placement.TryPlaceChara(ent, coordinates.ToMap(EntityManager), out actualCoordinates);
                    break;
                case PositionSearchType.General:
                default:
                    spatial.Coordinates = coordinates;
                    break;
            }

            FireGeneratingEvents(ent, coordinates, actualCoordinates, originalCount, args, container);

            if (!EntityManager.IsAlive(ent))
            {
                EntityManager.DeleteEntity(ent);

                Logger.WarningS("entity.gen", $"Entity {ent} became invalid after {nameof(EntityGeneratedEvent)} was fired.");
                return null;
            }

            if (commonArgs != null && !commonArgs.NoStack)
            {
                _stacks.TryStackAtSamePos(ent, showMessage: false);
            }

            return ent;
        }

        private enum PositionSearchType
        {
            General,
            Chara
        }

        private PositionSearchType GetSearchType(PrototypeId<EntityPrototype>? protoId)
        {
            if (protoId == null)
                return PositionSearchType.General;

            var proto = _protos.Index(protoId.Value);
            if (proto.Components.HasComponent<CharaComponent>())
                return PositionSearchType.Chara;

            return PositionSearchType.General;
        }

        public EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates, int? count = null, EntityGenArgSet? args = null)
            => SpawnEntityInternal(protoId, coordinates, count, args, container: null);

        public EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates, int? count = null, EntityGenArgSet? args = null)
        {
            if (!coordinates.TryToEntity(_mapManager, out var entityCoords))
            {
                Logger.ErrorS("entity.gen", $"Could not convert map coords {coordinates} to entity coords.");
                return null;
            }

            return SpawnEntity(protoId, entityCoords, count, args);
        }

        public EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityUid entity, int? count = null, EntityGenArgSet? args = null)
        {
            if (!TryComp<SpatialComponent>(entity, out var spatial))
            {
                return null;
            }

            return SpawnEntity(protoId, spatial.Coordinates, count, args);
        }

        public EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, IMap map, int? count = null, EntityGenArgSet? args = null)
        {
            var pos = _placement.FindFreePosition(map);
            if (pos == null)
                return null;

            return SpawnEntity(protoId, pos.Value, count, args);
        }

        public EntityUid? SpawnEntity(PrototypeId<EntityPrototype>? protoId, IContainer container, int? count = null, EntityGenArgSet? args = null)
        {
            var coords = new EntityCoordinates(container.Owner, Vector2i.Zero);
            var ent = SpawnEntityInternal(protoId, coords, count, args, container: container);

            if (!EntityManager.IsAlive(ent))
                return null;
        
            if (!container.Insert(ent.Value))
            {
                Logger.WarningS("entity.gen", $"Could not fit entity '{ent}' into container of entity '{container.Owner}'.");
                
                EntityManager.DeleteEntity(ent.Value);
                return null;
            }

            return ent.Value;
        }
    }

    /// <summary>
    /// Fired when an entity is being generated.
    /// </summary>
    /// <remarks>
    /// This event is *not* fired when an entity is cloned. Use this event when doing one-time
    /// random initialization to an entity.
    /// </remarks>
    [ByRefEvent]
    public struct EntityBeingGeneratedEvent
    {
        public EntityGenArgSet GenArgs { get; }
        public EntityGenCommonArgs CommonArgs => GenArgs.Get<EntityGenCommonArgs>();

        /// <summary>
        /// Coordinates the entity is being spawned at.
        /// </summary>
        public EntityCoordinates RequestedCoords { get; }
        
        /// <summary>
        /// Coordinates the entity was spawned at after adjustment from blocked tiles, etc.
        /// </summary>
        public EntityCoordinates Coords { get; }
        
        /// <summary>
        /// Amount passed to the generation routine. You can check this for null to randomly set the item's count.
        /// </summary>
        public int? OriginalAmount { get; }

        /// <summary>
        /// Container the entity will be inserted into after generation.
        /// </summary>
        public IContainer? Container { get; }

        public EntityBeingGeneratedEvent(EntityCoordinates requestedCoords, EntityCoordinates actualCoords, int? originalAmount, EntityGenArgSet genArgs, IContainer? container)
        {
            RequestedCoords = requestedCoords;
            Coords = actualCoords;
            OriginalAmount = originalAmount;
            GenArgs = genArgs;
            Container = container;
        }
    }

    /// <summary>
    /// Fired after an entity is generated.
    /// </summary>
    /// <remarks>
    /// NOTE: This event is also fired after an entity is cloned, so don't do any one-time random
    /// initialization to the entity in handlers to this event.
    /// </remarks>
    [ByRefEvent]
    public struct EntityGeneratedEvent
    {
        /// <summary>
        /// Coordinates the entity was spawned at.
        /// </summary>
        public EntityCoordinates RequestedCoords { get; }

        /// <summary>
        /// Coordinates the entity was spawned at after adjustment from blocked tiles, etc.
        /// </summary>
        public EntityCoordinates Coords { get; }

        /// <summary>
        /// Amount passed to the generation routine. You can check this for null to randomly set the item's count.
        /// </summary>
        public int? OriginalAmount { get; }

        /// <summary>
        /// Container the entity will be inserted into after generation.
        /// </summary>
        public IContainer? Container { get; }

        public EntityGeneratedEvent(EntityCoordinates requestedCoords, EntityCoordinates actualCoords, int? originalAmount, IContainer? container)
        {
            RequestedCoords = requestedCoords;
            Coords = actualCoords;
            OriginalAmount = originalAmount;
            Container = container;
        }
    }
}
