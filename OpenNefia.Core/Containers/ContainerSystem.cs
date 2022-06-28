using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Containers
{
    public sealed partial class ContainerSystem : EntitySystem
    {
        [Dependency] private readonly IDynamicTypeFactoryInternal _dynFactory = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            SubscribeBroadcast<EntityParentChangedEvent>(HandleParentChanged);
        }

        #region Container Management

        /// <summary>
        /// Makes a new container of the specified type.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID for the new container.</param>
        /// <typeparam name="T">The type of the new container</typeparam>
        /// <returns>The new container.</returns>
        /// <exception cref="ArgumentException">Thrown if there already is a container with the specified ID</exception>
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

        /// <summary>
        /// Ensures the container with the specified ID exists, creating it if necessary.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID to look up.</param>
        /// <returns>The container.</returns>
        public T EnsureContainer<T>(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null)
            where T : IContainer
        {
            if (!Resolve(uid, ref containerManager, false))
                containerManager = EntityManager.AddComponent<ContainerManagerComponent>(uid);

            if (TryGetContainer(uid, id, out var container, containerManager))
                return (T)container;

            return MakeContainer<T>(uid, id, containerManager);
        }

        /// <summary>
        /// Gets the container with the specified ID.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID to look up.</param>
        /// <returns>The container.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the container does not exist.</exception>
        public IContainer GetContainer(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                throw new ArgumentException("Entity does not have a ContainerManagerComponent!");

            return containerManager.Containers[id];
        }

        /// <summary>
        /// Checks whether we have a container with the specified ID.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The entity ID to check.</param>
        /// <returns>True if we already have a container, false otherwise.</returns>
        public bool HasContainer(EntityUid uid, ContainerId id, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                return false;

            return containerManager.Containers.ContainsKey(id);
        }

        /// <summary>
        /// Tries to get the container with the specified ID.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="id">The ID to look up.</param>
        /// <returns>The container.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the container does not exist.</exception>
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

        /// <summary>
        /// Attempt to retrieve a container that contains a specific entity.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="containedUid">The entity that is inside the container.</param>
        /// <param name="container">The container if it was found, <c>null</c> if not found.</param>
        /// <returns>True if the container was found, false otherwise.</returns>
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

        /// <summary>
        /// Attempts to retrieve a container that contains a specific entity.
        /// </summary>
        /// <param name="uid">The entity that is inside the container.</param>
        /// <param name="container">The container if it was found, <c>null</c> if not found.</param>
        /// <returns>True if the container was found, false otherwise.</returns>
        public bool TryGetContainingContainer(EntityUid uid, [NotNullWhen(true)] out IContainer? container, SpatialComponent? transform = null)
        {
            container = null;
            if (!Resolve(uid, ref transform, false))
                return false;

            if (!transform.ParentUid.IsValid())
                return false;

            return TryGetContainingContainer(transform.ParentUid, uid, out container);
        }

        /// <summary>
        /// Checks if this entity is in a container.
        /// </summary>
        /// <param name="uid">The entity to check for containment.</param>
        /// <returns>True if the entity is contained in a container.</returns>
        public bool IsEntityInContainer(EntityUid uid, SpatialComponent? transform = null)
        {
            return TryGetContainingContainer(uid, out _, transform);
        }

        /// <summary>
        /// Checks if this entity contains the specified entity.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <param name="containedUid">The entity to check for containment.</param>
        /// <returns>True if the entity is contained in the container.</returns>
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

        /// <summary>
        /// Attempts to remove <paramref name="entity" /> contained inside the owning entity,
        /// finding the container containing it automatically, if it is actually contained.
        /// </summary>
        /// <param name="uid">The owning container entity.</param>
        /// <param name="containedUid">The entity to remove.</param>
        /// <param name="force">Whether to forcefully remove the entity. Avoid using this if possible.</param>
        /// <returns>True if the entity was successfuly removed.</returns>
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

        /// <summary>
        /// Enumerates all containers on this entity.
        /// </summary>
        /// <param name="uid">The container entity.</param>
        /// <returns>A container enumerable.</returns>
        public ContainerManagerComponent.AllContainersEnumerable GetAllContainers(EntityUid uid, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                return new ContainerManagerComponent.AllContainersEnumerable();

            return containerManager.GetAllContainers();
        }

        #endregion

        #region Event Handlers

        // Eject entities from their parent container if the parent change is done by the transform only.
        private void HandleParentChanged(ref EntityParentChangedEvent message)
        {
            var oldParentEntity = message.OldParent;

            if (oldParentEntity == null || !EntityManager.EntityExists(oldParentEntity!.Value))
                return;

            if (EntityManager.TryGetComponent(oldParentEntity!.Value, out ContainerManagerComponent? containerManager))
                ForceRemove(message.EntityUid, containerManager);
        }

        #endregion
    }
}
