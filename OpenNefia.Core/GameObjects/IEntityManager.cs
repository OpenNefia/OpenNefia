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

        EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, EntityUid? euid,
            IEntityLoadContext? context = null);

        EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId,
            IEntityLoadContext? context = null);

        EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, EntityCoordinates coordinates,
            IEntityLoadContext? context = null);

        EntityUid CreateEntityUninitialized(PrototypeId<EntityPrototype>? prototypeId, MapCoordinates coordinates,
            IEntityLoadContext? context = null);

        /// <summary>
        /// Spawns an initialized entity at the default location, using the given prototype.
        /// </summary>
        /// <param name="protoId">The prototype to clone. If this is null, the entity won't have a prototype.</param>
        /// <param name="coordinates"></param>
        /// <returns>Newly created entity.</returns>
        EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates,
            IEntityLoadContext? context = null);

        /// <summary>
        /// Spawns an entity at a specific position
        /// </summary>
        /// <param name="protoName"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates,
            IEntityLoadContext? context = null);

        /// <summary>
        /// How many entities are currently active.
        /// </summary>
        int EntityCount { get; }

        /// <summary>
        /// Returns all entities by uid
        /// </summary>
        /// <returns></returns>
        IEnumerable<EntityUid> GetEntityUids();

        /// <summary>
        /// Shuts-down and removes the entity with the given <see cref="EntityUid"/>. This is also broadcast to all clients.
        /// </summary>
        /// <param name="uid">Uid of entity to remove.</param>
        void DeleteEntity(EntityUid uid);

        /// <summary>
        /// Checks whether an entity with the specified ID exists.
        /// </summary>
        bool EntityExists(EntityUid uid);

        /// <summary>
        /// Checks whether an entity with the specified ID has been deleted or is nonexistent.
        /// </summary>
        bool Deleted(EntityUid uid);

        /// <summary>
        /// Checks whether an entity with the specified ID has been deleted or is nonexistent.
        /// </summary>
        bool Deleted([NotNullWhen(false)] EntityUid? uid);

        /// <summary>
        /// Returns a string representation of an entity with various information regarding it.
        /// </summary>
        EntityStringRepresentation ToPrettyString(EntityUid uid);

        #endregion Entity Management
    }
}