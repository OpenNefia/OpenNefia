using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Web.XmlTransform;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.GameObjects
{
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
        private readonly HashSet<EntityUid> Entities = new();

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

        public EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, 
            EntityUid ? euid = null, IEntityLoadContext? context = null)
        {
            return CreateEntity(prototypeId, context, euid);
        }

        /// <inheritdoc />
        public EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId,
            IEntityLoadContext? context = null)
        {
            return CreateEntity(prototypeId, context);
        }

        /// <inheritdoc />
        public virtual EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, EntityCoordinates coordinates,
            IEntityLoadContext? context = null)
        {
            var newEntity = CreateEntity(prototypeId, context);

            if (coordinates.IsValid(this))
            {
                var spatial = EnsureComponent<SpatialComponent>(newEntity);
                spatial.Coordinates = coordinates;
            }

            return newEntity;
        }

        /// <inheritdoc />
        public virtual EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, MapCoordinates coordinates,
            IEntityLoadContext? context = null)
        {
            var newEntity = CreateEntity(prototypeId, context);
            var spatial = EnsureComponent<SpatialComponent>(newEntity);
            var mapEntityUid = _mapManager.GetMap(coordinates.MapId).MapEntityUid;
            spatial.AttachParent(GetComponent<SpatialComponent>(mapEntityUid));
            spatial.WorldPosition = coordinates.Position;
            return newEntity;
        }

        /// <inheritdoc />
        public virtual EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates,
            IEntityLoadContext? context = null)
        {
            if (!coordinates.IsValid(this))
                throw new InvalidOperationException($"Tried to spawn entity {protoId} on invalid coordinates {coordinates}.");

            var entity = CreateEntityUninitialized(protoId, coordinates, context);
            InitializeAndStartEntity(entity, coordinates.GetMapId(this));
            return entity;
        }

        /// <inheritdoc />
        public virtual EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates,
            IEntityLoadContext? context = null)
        {
            var entity = CreateEntityUninitialized(protoId, coordinates, context);
            InitializeAndStartEntity(entity, coordinates.MapId);
            return entity;
        }

        /// <inheritdoc />
        public int EntityCount => Entities.Count;

        public IEnumerable<EntityUid> GetEntityUids() => Entities;

        /// <summary>
        /// Shuts-down and removes given Entity. This is also broadcast to all clients.
        /// </summary>
        /// <param name="uid">Entity to remove</param>
        public void DeleteEntity(EntityUid e, EntityDeleteType deleteType = EntityDeleteType.Delete)
        {
            // Networking blindly spams entities at this function, they can already be
            // deleted from being a child of a previously deleted entity
            // TODO: Why does networking need to send deletes for child entities?
            if (!_entTraitArray[ArrayIndexFor<MetaDataComponent>()].TryGetValue(e, out var comp)
                || comp is not MetaDataComponent meta || meta.EntityDeleted)
                return;

            if (meta.EntityLifeStage >= EntityLifeStage.Terminating)
#if !EXCEPTION_TOLERANCE
                throw new InvalidOperationException("Called Delete on an entity already being deleted.");
#else
                return;
#endif

            RecursiveDeleteEntity(e, deleteType);
        }

        private EntityTerminatingEvent EntityTerminating = new();
        private EntityUnloadingEvent EntityUnloading = new();

        private void RecursiveDeleteEntity(EntityUid uid, EntityDeleteType deleteType)
        {
            if (Deleted(uid)) //TODO: Why was this still a child if it was already deleted?
                return;

            var spatial = GetComponent<SpatialComponent>(uid);
            var metadata = GetComponent<MetaDataComponent>(uid);
            metadata.EntityLifeStage = EntityLifeStage.Terminating;
            metadata.Liveness = EntityGameLiveness.DeadAndBuried;

            switch (deleteType)
            {
                case EntityDeleteType.Delete:
                default:
                    EventBus.RaiseEvent(uid, ref EntityTerminating, false);
                    break;
                case EntityDeleteType.Unload:
                    EventBus.RaiseEvent(uid, ref EntityUnloading, false);
                    break;
            }

            // DeleteEntity modifies our _children collection, we must cache the collection to iterate properly
            foreach (var child in spatial._children.ToArray())
            {
                // Recursion Alert
                RecursiveDeleteEntity(child, deleteType);
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
            switch (deleteType)
            {
                case EntityDeleteType.Delete:
                default:
                    EventBus.RaiseEvent(new EntityDeletedEvent(uid));
                    break;
                case EntityDeleteType.Unload:
                    EventBus.RaiseEvent(new EntityUnloadedEvent(uid));
                    break;
            }
            Entities.Remove(uid);
        }

        public bool EntityExists(EntityUid uid)
        {
            return _entTraitArray[ArrayIndexFor<MetaDataComponent>()].ContainsKey(uid);
        }

        public bool EntityExists(EntityUid? uid)
        {
            return uid.HasValue && EntityExists(uid.Value);
        }

        public bool Deleted(EntityUid uid)
        {
            return !_entTraitArray[ArrayIndexFor<MetaDataComponent>()].TryGetValue(uid, out var comp) || ((MetaDataComponent)comp).EntityDeleted;
        }

        public bool Deleted(EntityUid? uid)
        {
            return !uid.HasValue || !_entTraitArray[ArrayIndexFor<MetaDataComponent>()].TryGetValue(uid.Value, out var comp) || ((MetaDataComponent)comp).EntityDeleted;
        }

        /// <summary>
        /// Disposes all entities and clears all lists.
        /// </summary>
        public void FlushEntities(EntityDeleteType deleteType = EntityDeleteType.Delete)
        {
            foreach (var uid in GetEntityUids())
            {
                DeleteEntity(uid, deleteType);
            }

            // I would like the new entity UID to start from 0 when
            // going back to the title screen.
            NextEntityUid = (int)EntityUid.FirstUid;
        }

        /// <summary>
        ///     Allocates an entity and stores it but does not load components or do initialization.
        /// </summary>
        private protected EntityUid AllocEntity(PrototypeId<EntityPrototype>? prototypeId, EntityUid? uid = null)
        {
            EntityPrototype? prototype = null;
            if ((prototypeId?.IsValid() ?? false))
            {
                // If the prototype doesn't exist then we throw BEFORE we allocate the entity.
                prototype = PrototypeManager.Index(prototypeId.Value);
            }

            var entity = AllocEntity(uid);

            var metaData = GetComponent<MetaDataComponent>(entity);
            metaData.EntityPrototype = prototype;

            return entity;
        }

        /// <summary>
        ///     Allocates an entity and stores it but does not load components or do initialization.
        /// </summary>
        private protected EntityUid AllocEntity(EntityUid? uid = null)
        {
            if (uid == null)
            {
                uid = GenerateEntityUid();
            }

            if (EntityExists(uid.Value))
            {
                throw new InvalidOperationException($"UID already taken: {uid}");
            }

            var entity = uid.Value;

            // we want this called before adding components
            EntityAdded?.Invoke(this, entity);

            // We do this after the event, so if the event throws we have not committed
            Entities.Add(entity);

            // Create the MetaDataComponent and set it directly on the Entity to avoid a stack overflow in DEBUG.
            var metadata = new MetaDataComponent() { Owner = entity };
            metadata = IoCManager.InjectDependencies(metadata);

            // add the required MetaDataComponent directly.
            AddComponentInternal(uid.Value, metadata);

            // allocate the required SpatialComponent
            AddComponent<SpatialComponent>(entity);

            return entity;
        }

        /// <summary>
        ///     Allocates an entity and loads components but does not do initialization.
        /// </summary>
        private protected EntityUid CreateEntity(PrototypeId<EntityPrototype>? prototypeName,
            IEntityLoadContext? context = null, EntityUid? uid = null)
        {
            if (prototypeName == null)
                return AllocEntity(uid);

            var entity = AllocEntity(prototypeName, uid);
            try
            {
                var metaData = GetComponent<MetaDataComponent>(entity);
                _entityFactory.LoadEntity(metaData.EntityPrototype, entity, ComponentFactory, context);
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

        private protected void LoadEntity(EntityUid entity, IEntityLoadContext? context)
        {
            var metaData = GetComponent<MetaDataComponent>(entity);
            _entityFactory.LoadEntity(metaData.EntityPrototype, entity, ComponentFactory, context);
        }

        private void InitializeAndStartEntity(EntityUid uid, MapId mapId)
        {
            try
            {
                InitializeEntity(uid);
                StartEntity(uid);

                // If the map we're initializing the entity on is initialized, run map init on it.
                if (_mapManager.IsMapInitialized(mapId))
                    MapInitExt.RunMapInit(uid);
            }
            catch (Exception e)
            {
                DeleteEntity(uid);
                throw new EntityCreationException("Exception inside InitializeAndStartEntity", e);
            }
        }

        private protected void InitializeEntity(EntityUid uid)
        {
            InitializeComponents(uid);
            EntityInitialized?.Invoke(this, uid);
        }

        private protected void StartEntity(EntityUid uid)
        {
            StartComponents(uid);
            EntityStarted?.Invoke(this, uid);
        }

        /// <inheritdoc />
        public virtual EntityStringRepresentation ToPrettyString(EntityUid uid)
        {
            // We want to retrieve the MetaData component even if it is deleted.
            if (!_entTraitArray[ArrayIndexFor<MetaDataComponent>()].TryGetValue(uid, out var component))
                return new EntityStringRepresentation(uid, true);

            var metadata = (MetaDataComponent)component;

            // TODO
            var entityName = "<entity>";

            return new EntityStringRepresentation(uid, metadata.EntityDeleted, entityName, metadata.EntityPrototype?.ID);
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
