using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Containers
{
    public sealed partial class ContainerSystem : EntitySystem
    {    
        /// <summary>
        /// Am I inside a container?
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <returns>If the entity is inside of a container.</returns>
        public bool IsInContainer(EntityUid entity)
        {
            DebugTools.Assert(EntityManager.EntityExists(entity));

            // Notice the recursion starts at the Owner of the passed in entity, this
            // allows containers inside containers (toolboxes in lockers).
            if (EntityManager.GetComponent<SpatialComponent>(entity).Parent == null)
                return false;

            if (TryGetManagerComp(EntityManager.GetComponent<SpatialComponent>(entity).ParentUid, out var containerComp))
                return ContainsEntity(containerComp.OwnerUid, entity, containerComp);

            return false;
        }

        /// <summary>
        /// Tries to find the container manager that this entity is inside (if any).
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="manager">The container manager that this entity is inside of.</param>
        /// <returns>If a container manager was found.</returns>
        public bool TryGetContainerMan(EntityUid entity, [NotNullWhen(true)] out ContainerManagerComponent? manager)
        {
            DebugTools.Assert(EntityManager.EntityExists(entity));

            var parentTransform = EntityManager.GetComponent<SpatialComponent>(entity).Parent;
            if (parentTransform != null && TryGetManagerComp(parentTransform.OwnerUid, out manager)
                && ContainsEntity(manager.OwnerUid, entity, manager))
                return true;

            manager = default;
            return false;
        }

        /// <summary>
        /// Attempts to remove an entity from its container, if any.
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="force">Whether to forcibly remove the entity from the container.</param>
        /// <param name="wasInContainer">Whether the entity was actually inside a container or not.</param>
        /// <returns>If the entity could be removed. Also returns false if it wasn't inside a container.</returns>
        public bool TryRemoveFromContainer(EntityUid entity, bool force, out bool wasInContainer)
        {
            DebugTools.Assert(EntityManager.EntityExists(entity));

            if (TryGetContainingContainer(entity, out var container))
            {
                wasInContainer = true;

                if (!force)
                    return container.Remove(entity);

                container.ForceRemove(entity);
                return true;
            }

            wasInContainer = false;
            return false;
        }

        /// <summary>
        /// Attempts to remove an entity from its container, if any.
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="force">Whether to forcibly remove the entity from the container.</param>
        /// <returns>If the entity could be removed. Also returns false if it wasn't inside a container.</returns>
        public bool TryRemoveFromContainer(EntityUid entity, bool force = false)
        {
            return TryRemoveFromContainer(entity, force, out _);
        }

        /// <summary>
        /// Attempts to remove all entities in a container.
        /// </summary>
        public void EmptyContainer(IContainer container, bool force = false, EntityCoordinates? moveTo = null, bool attachToMap = false)
        {
            foreach (var entity in container.ContainedEntities.ToArray())
            {
                if (EntityManager.Deleted(entity))
                    continue;

                if (force)
                    container.ForceRemove(entity);
                else
                    container.Remove(entity);

                if (moveTo.HasValue)
                    EntityManager.GetComponent<SpatialComponent>(entity).Coordinates = moveTo.Value;

                if (attachToMap)
                    EntityManager.GetComponent<SpatialComponent>(entity).AttachToMap();
            }
        }

        /// <summary>
        /// Attempts to remove and delete all entities in a container.
        /// </summary>
        public void CleanContainer(IContainer container)
        {
            foreach (var ent in container.ContainedEntities.ToArray())
            {
                if (EntityManager.Deleted(ent)) continue;
                container.ForceRemove(ent);
                EntityManager.DeleteEntity(ent);
            }
        }

        public void AttachParentToContainerOrGrid(SpatialComponent transform)
        {
            if (transform.Parent == null
                || !TryGetContainingContainer(transform.Parent.OwnerUid, out var container)
                || !TryInsertIntoContainer(transform, container))
                transform.AttachToMap();
        }

        private bool TryInsertIntoContainer(SpatialComponent transform, IContainer container)
        {
            if (container.Insert(transform.OwnerUid)) return true;

            if (EntityManager.GetComponent<SpatialComponent>(container.Owner).Parent != null
                && TryGetContainingContainer(container.Owner, out var newContainer))
                return TryInsertIntoContainer(transform, newContainer);

            return false;
        }

        private bool TryGetManagerComp(EntityUid entity, [NotNullWhen(true)] out ContainerManagerComponent? manager)
        {
            DebugTools.Assert(EntityManager.EntityExists(entity));

            if (EntityManager.TryGetComponent(entity, out manager))
                return true;

            // RECURSION ALERT
            if (EntityManager.GetComponent<SpatialComponent>(entity).Parent != null)
                return TryGetManagerComp(EntityManager.GetComponent<SpatialComponent>(entity).ParentUid, out manager);

            return false;
        }

        public bool IsInSameOrNoContainer(EntityUid user, EntityUid other)
        {
            var isUserContained = TryGetContainingContainer(user, out var userContainer);
            var isOtherContained = TryGetContainingContainer(other, out var otherContainer);

            // Both entities are not in a container
            if (!isUserContained && !isOtherContained) return true;

            // Both entities are in different contained states
            if (isUserContained != isOtherContained) return false;

            // Both entities are in the same container
            return userContainer == otherContainer;
        }

        public bool IsInSameOrParentContainer(EntityUid user, EntityUid other)
        {
            var isUserContained = TryGetContainingContainer(user, out var userContainer);
            var isOtherContained = TryGetContainingContainer(other, out var otherContainer);

            // Both entities are not in a container
            if (!isUserContained && !isOtherContained) return true;

            // One contains the other
            if (userContainer?.Owner == other || otherContainer?.Owner == user) return true;

            // Both entities are in different contained states
            if (isUserContained != isOtherContained) return false;

            // Both entities are in the same container
            return userContainer == otherContainer;
        }

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
        public bool IsInSameOrTransparentContainer(EntityUid user, EntityUid other, bool userSeeInsideSelf = false)
        {
            DebugTools.AssertNotNull(user);
            DebugTools.AssertNotNull(other);

            TryGetContainingContainer(user, out IContainer? userContainer);
            TryGetContainingContainer(other, out IContainer? otherContainer);

            // Are both entities in the same container (or none)?
            if (userContainer == otherContainer) return true;

            // Is the user contained in the other entity?
            if (userContainer?.Owner == other) return true;

            // Does the user contain the other and can they see through themselves?
            if (userSeeInsideSelf && otherContainer?.Owner == user) return true;

            // Next we check for see-through containers. This uses some recursion, but it should be fine unless people
            // start spawning in glass matryoshka dolls.

            // Is the user in a see-through container?
            if (userContainer?.ShowContents ?? false)
                return IsInSameOrTransparentContainer(userContainer.Owner, other);

            // Is the other entity in a see-through container?
            if (otherContainer?.ShowContents ?? false)
                return IsInSameOrTransparentContainer(user, otherContainer.Owner);

            return false;
        }

        /// <summary>
        /// Shortcut method to make creation of containers easier.
        /// Creates a new container on the entity and gives it back to you.
        /// </summary>
        /// <param name="entity">The entity to create the container for.</param>
        /// <param name="containerId"></param>
        /// <returns>The new container.</returns>
        /// <exception cref="ArgumentException">Thrown if there already is a container with the specified ID.</exception>
        /// <seealso cref="IContainerManager.MakeContainer{T}(string)" />
        public T CreateContainer<T>(EntityUid entity, ContainerId containerId)
            where T : IContainer
        {
            if (!EntityManager.TryGetComponent<IContainerManager?>(entity, out var containerManager))
                containerManager = EntityManager.AddComponent<ContainerManagerComponent>(entity);

            return MakeContainer<T>(entity, containerId, (ContainerManagerComponent)containerManager);
        }

        public T EnsureContainer<T>(EntityUid entity, ContainerId containerId)
            where T : IContainer
        {
            return EnsureContainer<T>(entity, containerId, out _);
        }

        public T EnsureContainer<T>(EntityUid entity, ContainerId containerId, out bool alreadyExisted)
            where T : IContainer
        {
            var containerManager = EntityManager.EnsureComponent<ContainerManagerComponent>(entity);

            if (!TryGetContainer(entity, containerId, out var existing, containerManager))
            {
                alreadyExisted = false;
                return MakeContainer<T>(entity, containerId, containerManager);
            }

            if (!(existing is T container))
            {
                throw new InvalidOperationException(
                    $"The container exists but is of a different type: {existing.GetType()}");
            }

            alreadyExisted = true;
            return container;
        }
    }
}
