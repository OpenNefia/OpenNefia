using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.GameObjects
{
    public delegate void EntityQueryCallback(Entity entity);

    public delegate void EntityUidQueryCallback(EntityUid uid);

    /// <inheritdoc />
    public partial class EntityManager : IEntityManager
    {
        #region Dependencies

        [IoC.Dependency] private readonly IPrototypeManager PrototypeManager = default!;
        [IoC.Dependency] private readonly IEntitySystemManager EntitySystemManager = default!;
        [IoC.Dependency] private readonly IMapManager _mapManager = default!;
        [IoC.Dependency] private readonly IEntityFactoryInternal _entityFactory = default!;

        #endregion Dependencies

        IComponentFactory IEntityManager.ComponentFactory => ComponentFactory;

        /// <inheritdoc />
        public IEntitySystemManager EntitySysManager => EntitySystemManager;

        private readonly Queue<EntityUid> QueuedDeletions = new();
        private readonly HashSet<EntityUid> QueuedDeletionsSet = new();

        /// <summary>
        ///     All entities currently stored in the manager.
        /// </summary>
        private readonly Dictionary<EntityUid, Entity> Entities = new();

        private EntityEventBus _eventBus = null!;

        /// <inheritdoc/>
        public int NextEntityUid { get; set; } = (int)EntityUid.FirstUid;

        /// <inheritdoc />
        public IEventBus EventBus => _eventBus;

        public event EventHandler<EntityUid>? EntityAdded;
        public event EventHandler<EntityUid>? EntityInitialized;
        public event EventHandler<EntityUid>? EntityStarted;
        public event EventHandler<EntityUid>? EntityDeleted;

        public bool Started { get; private set; }
        public bool Initialized { get; private set; }

        /// <summary>
        /// Constructs a new instance of <see cref="EntityManager"/>.
        /// </summary>
        public EntityManager()
        {
        }

        public void Initialize()
        {
            if (Initialized)
                throw new InvalidOperationException("Initialize() called multiple times");

            _eventBus = new EntityEventBus(this);

            InitializeComponents();

            Initialized = true;
        }

        public void Startup()
        {
            if (Started)
                throw new InvalidOperationException("Startup() called multiple times");

            EntitySystemManager.Initialize();
            Started = true;
        }

        public void Shutdown()
        {
            FlushEntities();
            _eventBus.ClearEventTables();
            EntitySystemManager.Shutdown();
            ClearComponents();
            Initialized = false;
            Started = false;
        }

        public void Cleanup()
        {
            QueuedDeletions.Clear();
            QueuedDeletionsSet.Clear();
            Entities.Clear();
            _eventBus.Dispose();
            _eventBus = null!;
            ClearComponents();

            Initialized = false;
            Started = false;
        }

        #region Entity Management

        public Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, EntityUid? euid)
        {
            return CreateEntity(prototypeId, euid);
        }

        /// <inheritdoc />
        public Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId)
        {
            return CreateEntity(prototypeId);
        }

        /// <inheritdoc />
        public virtual Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, EntityCoordinates coordinates)
        {
            var newEntity = CreateEntity(prototypeId);

            if (coordinates.IsValid(this))
            {
                newEntity.Spatial.Coordinates = coordinates;
            }

            return newEntity;
        }

        /// <inheritdoc />
        public virtual Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, MapCoordinates coordinates)
        {
            var newEntity = CreateEntity(prototypeId);
            newEntity.Spatial.AttachParent(_mapManager.GetMapEntity(coordinates.MapId));
            newEntity.Spatial.WorldPosition = coordinates.Position;
            return newEntity;
        }

        /// <inheritdoc />
        public virtual Entity SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates)
        {
            if (!coordinates.IsValid(this))
                throw new InvalidOperationException($"Tried to spawn entity {protoId} on invalid coordinates {coordinates}.");

            var entity = CreateEntityUninitialized(protoId, coordinates);
            InitializeAndStartEntity(entity, coordinates.GetMapId(this));
            return entity;
        }

        /// <inheritdoc />
        public virtual Entity SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates)
        {
            var entity = CreateEntityUninitialized(protoId, coordinates);
            InitializeAndStartEntity(entity, coordinates.MapId);
            return entity;
        }

        /// <summary>
        /// Returns an entity by id
        /// </summary>
        /// <param name="uid"></param>
        /// <returns>Entity or throws if the entity doesn't exist</returns>
        public Entity GetEntity(EntityUid uid)
        {
            return Entities[uid];
        }

        /// <summary>
        /// Attempt to get an entity, returning whether or not an entity was gotten.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="entity">The requested entity or null if the entity couldn't be found.</param>
        /// <returns>True if a value was returned, false otherwise.</returns>
        public bool TryGetEntity(EntityUid uid, [NotNullWhen(true)] out Entity? entity)
        {
            if (Entities.TryGetValue(uid, out var cEntity) && !cEntity.Deleted)
            {
                entity = cEntity;
                return true;
            }

            // entity might get assigned if it's deleted but still found,
            // prevent somebody from being "smart".
            entity = null;
            return false;
        }

        /// <inheritdoc />
        public int EntityCount => Entities.Count;

        /// <inheritdoc />
        public IEnumerable<Entity> GetEntities() => Entities.Values;

        public IEnumerable<EntityUid> GetEntityUids() => Entities.Keys;

        /// <summary>
        /// Shuts-down and removes given Entity. This is also broadcast to all clients.
        /// </summary>
        /// <param name="e">Entity to remove</param>
        public void DeleteEntity(Entity e)
        {
            // Networking blindly spams entities at this function, they can already be
            // deleted from being a child of a previously deleted entity
            // TODO: Why does networking need to send deletes for child entities?
            if (e.Deleted)
                return;

            if (e.LifeStage >= EntityLifeStage.Terminating)
#if !EXCEPTION_TOLERANCE
                throw new InvalidOperationException("Called Delete on an entity already being deleted.");
#else
                return;
#endif

            RecursiveDeleteEntity(e.Uid);
        }

        private EntityTerminatingEvent EntityTerminating = new();

        private void RecursiveDeleteEntity(EntityUid uid)
        {
            if (Deleted(uid)) //TODO: Why was this still a child if it was already deleted?
                return;

            var spatial = GetComponent<SpatialComponent>(uid);
            var metadata = GetComponent<MetaDataComponent>(uid);
            metadata.EntityLifeStage = EntityLifeStage.Terminating;
            metadata.Liveness = EntityGameLiveness.DeadAndBuried;

            EventBus.RaiseLocalEvent(uid, ref EntityTerminating, false);

            // DeleteEntity modifies our _children collection, we must cache the collection to iterate properly
            foreach (var childTransform in spatial.Children.ToArray())
            {
                // Recursion Alert
                RecursiveDeleteEntity(childTransform.OwnerUid);
            }

            // Shut down all components.
            foreach (var component in InSafeOrder(_entCompIndex[uid]))
            {
                if (component.Running)
                    component.LifeShutdown();
            }

            // map does not have a parent node, everything else needs to be detached
            if (spatial.ParentUid != EntityUid.Invalid)
            {
                // Detach from my parent, if any
                spatial.DetachParentToNull();
            }

            // Dispose all my components, in a safe order so transform is available
            DisposeComponents(uid);

            metadata.EntityLifeStage = EntityLifeStage.Deleted;
            EntityDeleted?.Invoke(this, uid);
            EventBus.RaiseEvent(EventSource.Local, new EntityDeletedEvent(uid));
            Entities.Remove(uid);
        }

        public void QueueDeleteEntity(Entity entity)
        {
            QueueDeleteEntity(entity.Uid);
        }

        public void QueueDeleteEntity(EntityUid uid)
        {
            if(QueuedDeletionsSet.Add(uid))
                QueuedDeletions.Enqueue(uid);
        }

        public void DeleteEntity(EntityUid uid)
        {
            if (TryGetEntity(uid, out var entity))
            {
                DeleteEntity(entity);
            }
        }

        public bool EntityExists(EntityUid uid)
        {
            return TryGetEntity(uid, out _);
        }

        public bool Deleted(EntityUid uid)
        {
            return !_entTraitDict[typeof(MetaDataComponent)].TryGetValue(uid, out var comp) || ((MetaDataComponent)comp).EntityDeleted;
        }

        public bool Deleted(EntityUid? uid)
        {
            return !uid.HasValue || !_entTraitDict[typeof(MetaDataComponent)].TryGetValue(uid.Value, out var comp) || ((MetaDataComponent)comp).EntityDeleted;
        }

        /// <summary>
        /// Disposes all entities and clears all lists.
        /// </summary>
        public void FlushEntities()
        {
            foreach (var e in GetEntities())
            {
                DeleteEntity(e);
            }

            // I would like the new entity UID to start from 0 when
            // going back to the title screen.
            NextEntityUid = (int)EntityUid.FirstUid;
        }

        /// <summary>
        ///     Allocates an entity and stores it but does not load components or do initialization.
        /// </summary>
        private protected Entity AllocEntity(PrototypeId<EntityPrototype>? prototypeId, EntityUid? uid = null)
        {
            EntityPrototype? prototype = null;
            if ((prototypeId?.IsValid() ?? false))
            {
                // If the prototype doesn't exist then we throw BEFORE we allocate the entity.
                prototype = PrototypeManager.Index(prototypeId.Value);
            }

            var entity = AllocEntity(uid);

            entity.Prototype = prototype;

            return entity;
        }

        /// <summary>
        ///     Allocates an entity and stores it but does not load components or do initialization.
        /// </summary>
        private protected Entity AllocEntity(EntityUid? uid = null)
        {
            if (uid == null)
            {
                uid = GenerateEntityUid();
            }

            if (EntityExists(uid.Value))
            {
                throw new InvalidOperationException($"UID already taken: {uid}");
            }

            var entity = new Entity(this, uid.Value);

            // we want this called before adding components
            EntityAdded?.Invoke(this, entity.Uid);

            // We do this after the event, so if the event throws we have not committed
            Entities[entity.Uid] = entity;

            // Create the MetaDataComponent and set it directly on the Entity to avoid a stack overflow in DEBUG.
            var metadata = new MetaDataComponent() { Owner = entity };
            metadata = IoCManager.InjectDependencies(metadata);
            entity.MetaData = metadata;

            // add the required MetaDataComponent directly.
            AddComponentInternal(uid.Value, metadata);

            // allocate the required SpatialComponent
            AddComponent<SpatialComponent>(entity);

            return entity;
        }

        /// <summary>
        ///     Allocates an entity and loads components but does not do initialization.
        /// </summary>
        private protected Entity CreateEntity(PrototypeId<EntityPrototype>? prototypeName, EntityUid? uid = null)
        {
            if (prototypeName == null)
                return AllocEntity(uid);

            var entity = AllocEntity(prototypeName, uid);
            try
            {
                _entityFactory.LoadEntity(entity.Prototype, entity, ComponentFactory, null);
                return entity;
            }
            catch (Exception e)
            {
                // Exception during entity loading.
                // Need to delete the entity to avoid corrupt state causing crashes later.
                DeleteEntity(entity);
                throw new EntityCreationException($"Exception inside CreateEntity with prototype {prototypeName}", e);
            }
        }

        private protected void LoadEntity(Entity entity, IEntityLoadContext? context)
        {
            _entityFactory.LoadEntity(entity.Prototype, entity, ComponentFactory, context);
        }

        private void InitializeAndStartEntity(Entity entity, MapId mapId)
        {
            try
            {
                InitializeEntity(entity);
                StartEntity(entity);

                // If the map we're initializing the entity on is initialized, run map init on it.
                if (_mapManager.IsMapInitialized(mapId))
                    MapInitExt.RunMapInit(entity.Uid);
            }
            catch (Exception e)
            {
                DeleteEntity(entity);
                throw new EntityCreationException("Exception inside InitializeAndStartEntity", e);
            }
        }

        private protected void InitializeEntity(Entity entity)
        {
            InitializeComponents(entity.Uid);
            EntityInitialized?.Invoke(this, entity.Uid);
        }

        private protected void StartEntity(Entity entity)
        {
            StartComponents(entity.Uid);
            EntityStarted?.Invoke(this, entity.Uid);
        }

#endregion Entity Management

        /// <summary>
        ///     Factory for generating a new EntityUid for an entity currently being created.
        /// </summary>
        /// <inheritdoc />
        private EntityUid GenerateEntityUid()
        {
            return new(NextEntityUid++);
        }
    }

    public enum EntityMessageType : byte
    {
        Error = 0,
        ComponentMessage,
        SystemMessage
    }
}
