using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Containers
{
    public interface IContainerSystem : IEntitySystem
    {
        /// <summary>
        /// Makes a new container of the specified type.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID for the new container.</param>
        /// <typeparam name="T">The type of the new container</typeparam>
        /// <returns>The new container.</returns>
        /// <exception cref="ArgumentException">Thrown if there already is a container with the specified ID</exception>
        T MakeContainer<T>(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null) where T : IContainer;

        /// <summary>
        /// Ensures the container with the specified ID exists, creating it if necessary.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID to look up.</param>
        /// <returns>The container.</returns>
        T EnsureContainer<T>(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null) where T : IContainer;

        T EnsureContainer<T>(EntityUid entity, ContainerId containerId) where T : IContainer;

        T EnsureContainer<T>(EntityUid entity, ContainerId containerId, out bool alreadyExisted) where T : IContainer;

        /// <summary>
        /// Gets the container with the specified ID.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID to look up.</param>
        /// <returns>The container.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the container does not exist.</exception>
        IContainer GetContainer(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null);

        /// <summary>
        /// Checks whether we have a container with the specified ID.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The entity ID to check.</param>
        /// <returns>True if we already have a container, false otherwise.</returns>
        bool HasContainer(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null);

        /// <summary>
        /// Tries to get the container with the specified ID.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID to look up.</param>
        /// <returns>The container.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the container does not exist.</exception>
        bool TryGetContainer(EntityUid uid, ContainerId id, [NotNullWhen(true)] out IContainer? container, ContainerManagerComponent? containerManager = null);

        /// <summary>
        /// Attempt to retrieve a container that contains a specific entity.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="containedUid">The entity that is inside the container.</param>
        /// <param name="container">The container if it was found, <c>null</c> if not found.</param>
        /// <returns>True if the container was found, false otherwise.</returns>
        bool TryGetContainingContainer(EntityUid uid, EntityUid containedUid, [NotNullWhen(true)] out IContainer? container, ContainerManagerComponent? containerManager = null);

        /// <summary>
        /// Attempts to retrieve a container that contains a specific entity.
        /// </summary>
        /// <param name="uid">The entity that is inside the container.</param>
        /// <param name="container">The container if it was found, <c>null</c> if not found.</param>
        /// <returns>True if the container was found, false otherwise.</returns>
        bool TryGetContainingContainer(EntityUid uid, [NotNullWhen(true)] out IContainer? container, SpatialComponent? transform = null);

        /// <summary>
        /// Checks if this entity is in a container.
        /// </summary>
        /// <param name="uid">The entity to check for containment.</param>
        /// <returns>True if the entity is contained in a container.</returns>
        bool IsEntityInContainer(EntityUid uid, SpatialComponent? transform = null);


        /// <summary>
        /// Checks if this entity contains the specified entity.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="containedUid">The entity to check for containment.</param>
        /// <returns>True if the entity is contained in the container.</returns>
        bool ContainsEntity(EntityUid uid, EntityUid containedUid, ContainerManagerComponent? containerManager = null);

        /// <summary>
        /// Attempts to remove <paramref name="entity" /> contained inside the owning entity,
        /// finding the container containing it automatically, if it is actually contained.
        /// </summary>
        /// <param name="uid">The owning container entity.</param>
        /// <param name="containedUid">The entity to remove.</param>
        /// <param name="force">Whether to forcefully remove the entity. Avoid using this if possible.</param>
        /// <returns>True if the entity was successfuly removed.</returns>
        bool RemoveEntity(EntityUid uid, EntityUid containedUid, bool force = false, ContainerManagerComponent? containerManager = null);

        /// <summary>
        /// Enumerates all containers on this entity.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <returns>A container enumerable.</returns>
        ContainerManagerComponent.AllContainersEnumerable GetAllContainers(EntityUid uid, ContainerManagerComponent? containerManager = null);

        /// <summary>
        /// Tries to find the container manager that this entity is inside (if any).
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="manager">The container manager that this entity is inside of.</param>
        /// <returns>If a container manager was found.</returns>
        bool TryGetContainerMan(EntityUid entity, [NotNullWhen(true)] out ContainerManagerComponent? manager);
        
        /// <summary>
        /// Attempts to remove an entity from its container, if any.
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="force">Whether to forcibly remove the entity from the container.</param>
        /// <param name="wasInContainer">Whether the entity was actually inside a container or not.</param>
        /// <returns>If the entity could be removed. Also returns false if it wasn't inside a container.</returns>
        bool TryRemoveFromContainer(EntityUid entity, bool force, out bool wasInContainer);

        /// <summary>
        /// Attempts to remove an entity from its container, if any.
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="force">Whether to forcibly remove the entity from the container.</param>
        /// <returns>If the entity could be removed. Also returns false if it wasn't inside a container.</returns>
        bool TryRemoveFromContainer(EntityUid entity, bool force = false);

        /// <summary>
        /// Attempts to remove all entities in a container.
        /// </summary>
        void EmptyContainer(IContainer container, bool force = false, EntityCoordinates? moveTo = null, bool attachToMap = false);
        
        /// <summary>
        /// Attempts to remove and delete all entities in a container.
        /// </summary>
        void CleanContainer(IContainer container);

        void AttachParentToContainerOrMap(SpatialComponent transform);
        
        bool IsInSameOrNoContainer(EntityUid user, EntityUid other);
        
        bool IsInSameOrParentContainer(EntityUid user, EntityUid other);
        
        /// <summary>
        ///     Check whether a given entity can see another entity despite whatever containers they may be in.
        /// </summary>
        /// <remarks>
        ///     This is effectively a variant of <see cref="IsInSameOrParentContainer"/> that also checks whether the
        ///     containers are transparent. Additionally, an entity can "see" the entity that contains it, but unless
        ///     otherwise specified the containing entity cannot see into itself. For example, a human in a locker can
        ///     see the locker and other items in that locker, but the human cannot see their own organs.  Note that
        ///     this means that the two entity arguments are NOT interchangeable.
        /// </remarks>
        bool IsInSameOrTransparentContainer(EntityUid user, EntityUid other, bool userSeeInsideSelf = false);
    }

    public sealed partial class ContainerSystem : EntitySystem, IContainerSystem
    {
        [Dependency] private readonly IDynamicTypeFactoryInternal _dynFactory = default!;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            SubscribeEntity<EntityParentChangedEvent>(HandleParentChanged);
        }

        #region Container Management

        /// <inheritdoc />
        public T MakeContainer<T>(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null)
            where T : IContainer
        {
            if (!Resolve(uid, ref containerManager, false))
                containerManager = EntityManager.AddComponent<ContainerManagerComponent>(uid); // Happy Vera.

            return (T)MakeContainer(uid, id, typeof(T), containerManager);
        }

        private IContainer MakeContainer(EntityUid uid, ContainerId id, Type type, ContainerManagerComponent containerManager)
        {
            if (HasContainer(uid, id, containerManager)) throw new ArgumentException($"Container with specified ID already exists: '{id}'");

            var container = _dynFactory.CreateInstanceUnchecked<BaseContainer>(type);
            container.ID = id;
            container.Manager = containerManager;

            containerManager._containers[id] = container;
            return container;
        }

        /// <inheritdoc />
        public T EnsureContainer<T>(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null)
            where T : IContainer
        {
            if (!Resolve(uid, ref containerManager, false))
                containerManager = EntityManager.AddComponent<ContainerManagerComponent>(uid);

            if (TryGetContainer(uid, id, out var container, containerManager))
                return (T)container;

            return MakeContainer<T>(uid, id, containerManager);
        }

        /// <inheritdoc />
        public IContainer GetContainer(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                throw new ArgumentException("Entity does not have a ContainerManagerComponent!");

            return containerManager.Containers[id];
        }

        /// <inheritdoc />
        public bool HasContainer(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                return false;

            return containerManager.Containers.ContainsKey(id);
        }

        /// <inheritdoc />
        public bool TryGetContainer(EntityUid uid, ContainerId id, [NotNullWhen(true)] out IContainer? container, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager, false))
            {
                container = null;
                return false;
            }

            var ret = containerManager.Containers.TryGetValue(id, out var cont);
            container = cont!;
            return ret;
        }

        /// <inheritdoc />
        public bool TryGetContainingContainer(EntityUid uid, EntityUid containedUid, [NotNullWhen(true)] out IContainer? container, ContainerManagerComponent? containerManager = null)
        {
            if (!(Resolve(uid, ref containerManager, false) && EntityManager.EntityExists(containedUid)))
            {
                container = null;
                return false;
            }

            foreach (var contain in containerManager.Containers.Values)
            {
                if (!contain.Deleted && contain.Contains(containedUid))
                {
                    container = contain;
                    return true;
                }
            }

            container = default;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetContainingContainer(EntityUid uid, [NotNullWhen(true)] out IContainer? container, SpatialComponent? transform = null)
        {
            container = null;
            if (!Resolve(uid, ref transform, false))
                return false;

            if (!transform.ParentUid.IsValid())
                return false;

            return TryGetContainingContainer(transform.ParentUid, uid, out container);
        }

        /// <inheritdoc />
        public bool IsEntityInContainer(EntityUid uid, SpatialComponent? transform = null)
        {
            return TryGetContainingContainer(uid, out _, transform);
        }

        /// <inheritdoc />
        public bool ContainsEntity(EntityUid uid, EntityUid containedUid, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager, false) || !EntityManager.EntityExists(containedUid))
                return false;

            foreach (var container in containerManager.Containers.Values)
            {
                if (!container.Deleted && container.Contains(containedUid)) return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool RemoveEntity(EntityUid uid, EntityUid containedUid, bool force = false, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager) || !EntityManager.EntityExists(containedUid))
                return false;

            if (force)
            {
                ForceRemove(containedUid, containerManager);
                return true;
            }
            else
            {
                return Remove(containedUid, containerManager);
            }
        }

        private void ForceRemove(EntityUid containedUid, ContainerManagerComponent containerManager)
        {
            foreach (var container in containerManager.Containers.Values)
            {
                if (container.Contains(containedUid)) container.ForceRemove(containedUid);
            }
        }

        private bool Remove(EntityUid containedUid, ContainerManagerComponent containerManager)
        {
            foreach (var containers in containerManager.Containers.Values)
            {
                if (containers.Contains(containedUid)) return containers.Remove(containedUid);
            }

            return true; // If we don't contain the entity, it will always be removed
        }

        /// <inheritdoc />
        public ContainerManagerComponent.AllContainersEnumerable GetAllContainers(EntityUid uid, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                return new ContainerManagerComponent.AllContainersEnumerable();

            return containerManager.GetAllContainers();
        }

        #endregion

        #region Event Handlers

        // Eject entities from their parent container if the parent change is done by the transform only.
        private void HandleParentChanged(EntityUid entity, ref EntityParentChangedEvent message)
        {
            var oldParentEntity = message.OldParent;

            if (oldParentEntity == null || !EntityManager.EntityExists(oldParentEntity!.Value))
                return;

            if (EntityManager.TryGetComponent(oldParentEntity!.Value, out ContainerManagerComponent? containerManager))
                ForceRemove(entity, containerManager);
        }

        #endregion
    }
}
