using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.GameObjects
{
    public partial interface IEntityManager
    {        
        void Initialize();
        void Startup();
        void Shutdown();

        /// <summary>
        ///     Drops every entity, component and entity system.
        /// </summary>
        void Cleanup();

        IComponentFactory ComponentFactory { get; }
        IEntitySystemManager EntitySysManager { get; }
        IEventBus EventBus { get; }

        #region Entity Management

        event EventHandler<EntityUid>? EntityAdded;
        event EventHandler<EntityUid>? EntityInitialized;
        event EventHandler<EntityUid>? EntityStarted;
        event EventHandler<EntityUid>? EntityDeleted;

        Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, EntityUid? euid);

        Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId);

        Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, EntityCoordinates coordinates);

        Entity CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, MapCoordinates coordinates);

        /// <summary>
        /// Spawns an initialized entity at the default location, using the given prototype.
        /// </summary>
        /// <param name="protoId">The prototype to clone. If this is null, the entity won't have a prototype.</param>
        /// <param name="coordinates"></param>
        /// <returns>Newly created entity.</returns>
        Entity SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates);

        /// <summary>
        /// Spawns an entity at a specific position
        /// </summary>
        /// <param name="protoName"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        Entity SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates);

        /// <summary>
        /// Returns an entity by id
        /// </summary>
        /// <param name="uid"></param>
        /// <returns>Entity or throws if entity id doesn't exist</returns>
        Entity GetEntity(EntityUid uid);

        /// <summary>
        /// Attempt to get an entity, returning whether or not an entity was gotten.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="entity">The requested entity or null if the entity couldn't be found.</param>
        /// <returns>True if a value was returned, false otherwise.</returns>
        bool TryGetEntity(EntityUid uid, [NotNullWhen(true)] out Entity? entity);

        /// <summary>
        /// How many entities are currently active.
        /// </summary>
        int EntityCount { get; }

        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <returns></returns>
        IEnumerable<Entity> GetEntities();

        /// <summary>
        /// Returns all entities by uid
        /// </summary>
        /// <returns></returns>
        IEnumerable<EntityUid> GetEntityUids();

        /// <summary>
        /// Shuts-down and removes given <see cref="Entity"/>. This is also broadcast to all clients.
        /// </summary>
        /// <param name="e">Entity to remove</param>
        void DeleteEntity(Entity e);

        /// <summary>
        /// Shuts-down and removes the entity with the given <see cref="EntityUid"/>. This is also broadcast to all clients.
        /// </summary>
        /// <param name="uid">Uid of entity to remove.</param>
        void DeleteEntity(EntityUid uid);

        /// <summary>
        /// Checks whether an entity with the specified ID exists.
        /// </summary>
        bool EntityExists(EntityUid uid);

        #endregion Entity Management
    }
}