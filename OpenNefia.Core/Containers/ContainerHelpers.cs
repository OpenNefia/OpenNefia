using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Helper functions for the container system.
    /// </summary>
    [PublicAPI]
    [Obsolete("move to system")]
    public static class ContainerHelpers
    {
        /// <summary>
        /// Am I inside a container?
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <returns>If the entity is inside of a container.</returns>
        public static bool IsInContainer(this EntityUid entity)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            DebugTools.Assert(entMan.EntityExists(entity));

            // Notice the recursion starts at the Owner of the passed in entity, this
            // allows containers inside containers (toolboxes in lockers).
            if (entMan.GetComponent<SpatialComponent>(entity).Parent == null)
                return false;

            if (TryGetManagerComp(entMan.GetComponent<SpatialComponent>(entity).ParentUid, out var containerComp))
                return containerComp.ContainsEntity(entity);

            return false;
        }

        /// <summary>
        /// Tries to find the container manager that this entity is inside (if any).
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="manager">The container manager that this entity is inside of.</param>
        /// <returns>If a container manager was found.</returns>
        public static bool TryGetContainerMan(this EntityUid entity, [NotNullWhen(true)] out IContainerManager? manager)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            DebugTools.Assert(entMan.EntityExists(entity));

            var parentTransform = entMan.GetComponent<SpatialComponent>(entity).Parent;
            if (parentTransform != null && TryGetManagerComp(parentTransform.OwnerUid, out manager) && manager.ContainsEntity(entity))
                return true;

            manager = default;
            return false;
        }

        /// <summary>
        /// Tries to find the container that this entity is inside (if any).
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="container">The container that this entity is inside of.</param>
        /// <returns>If a container was found.</returns>
        public static bool TryGetContainer(this EntityUid entity, [NotNullWhen(true)] out IContainer? container)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            DebugTools.Assert(entMan.EntityExists(entity));

            if (TryGetContainerMan(entity, out var manager))
                return manager.TryGetContainer(entity, out container);

            container = default;
            return false;
        }

        /// <summary>
        /// Attempts to remove an entity from its container, if any.
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="force">Whether to forcibly remove the entity from the container.</param>
        /// <param name="wasInContainer">Whether the entity was actually inside a container or not.</param>
        /// <returns>If the entity could be removed. Also returns false if it wasn't inside a container.</returns>
        public static bool TryRemoveFromContainer(this EntityUid entity, bool force, out bool wasInContainer)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            DebugTools.Assert(entMan.EntityExists(entity));

            if (TryGetContainer(entity, out var container))
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
        public static bool TryRemoveFromContainer(this EntityUid entity, bool force = false)
        {
            return TryRemoveFromContainer(entity, force, out _);
        }

        /// <summary>
        /// Attempts to remove all entities in a container.
        /// </summary>
        public static void EmptyContainer(this IContainer container, bool force = false, EntityCoordinates? moveTo = null, bool attachToMap = false)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            foreach (var entity in container.ContainedEntities.ToArray())
            {
                if (entMan.Deleted(entity))
                    continue;

                if (force)
                    container.ForceRemove(entity);
                else
                    container.Remove(entity);

                if (moveTo.HasValue)
                    entMan.GetComponent<SpatialComponent>(entity).Coordinates = moveTo.Value;

                if (attachToMap)
                    entMan.GetComponent<SpatialComponent>(entity).AttachToMap();
            }
        }

        /// <summary>
        /// Attempts to remove and delete all entities in a container.
        /// </summary>
        public static void CleanContainer(this IContainer container)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            foreach (var ent in container.ContainedEntities.ToArray())
            {
                if (entMan.Deleted(ent)) continue;
                container.ForceRemove(ent);
                entMan.DeleteEntity(ent);
            }
        }

        public static void AttachParentToContainerOrGrid(this SpatialComponent transform)
        {
            if (transform.Parent == null
                || !TryGetContainer(transform.Parent.OwnerUid, out var container)
                || !TryInsertIntoContainer(transform, container))
                transform.AttachToMap();
        }

        private static bool TryInsertIntoContainer(this SpatialComponent transform, IContainer container)
        {
            if (container.Insert(transform.OwnerUid)) return true;

            var entMan = IoCManager.Resolve<IEntityManager>();
            if (entMan.GetComponent<SpatialComponent>(container.Owner).Parent != null
                && TryGetContainer(container.Owner, out var newContainer))
                return TryInsertIntoContainer(transform, newContainer);

            return false;
        }

        private static bool TryGetManagerComp(this EntityUid entity, [NotNullWhen(true)] out IContainerManager? manager)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            DebugTools.Assert(entMan.EntityExists(entity));

            if (entMan.TryGetComponent(entity, out manager))
                return true;

            // RECURSION ALERT
            if (entMan.GetComponent<SpatialComponent>(entity).Parent != null)
                return TryGetManagerComp(entMan.GetComponent<SpatialComponent>(entity).ParentUid, out manager);

            return false;
        }

        public static bool IsInSameOrNoContainer(this EntityUid user, EntityUid other)
        {
            var isUserContained = TryGetContainer(user, out var userContainer);
            var isOtherContained = TryGetContainer(other, out var otherContainer);

            // Both entities are not in a container
            if (!isUserContained && !isOtherContained) return true;

            // Both entities are in different contained states
            if (isUserContained != isOtherContained) return false;

            // Both entities are in the same container
            return userContainer == otherContainer;
        }

        public static bool IsInSameOrParentContainer(this EntityUid user, EntityUid other)
        {
            var isUserContained = TryGetContainer(user, out var userContainer);
            var isOtherContained = TryGetContainer(other, out var otherContainer);

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
        public static bool IsInSameOrTransparentContainer(this EntityUid user, EntityUid other, bool userSeeInsideSelf = false)
        {
            DebugTools.AssertNotNull(user);
            DebugTools.AssertNotNull(other);

            TryGetContainer(user, out IContainer? userContainer);
            TryGetContainer(other, out IContainer? otherContainer);

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
        public static T CreateContainer<T>(this EntityUid entity, string containerId)
            where T : IContainer
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            if (!entMan.TryGetComponent<IContainerManager?>(entity, out var containermanager))
                containermanager = entMan.AddComponent<ContainerManagerComponent>(entity);

            return containermanager.MakeContainer<T>(containerId);
        }

        public static T EnsureContainer<T>(this EntityUid entity, string containerId)
            where T : IContainer
        {
            return EnsureContainer<T>(entity, containerId, out _);
        }

        public static T EnsureContainer<T>(this EntityUid entity, string containerId, out bool alreadyExisted)
            where T : IContainer
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            var containerManager = entMan.EnsureComponent<ContainerManagerComponent>(entity);

            if (!containerManager.TryGetContainer(containerId, out var existing))
            {
                alreadyExisted = false;
                return containerManager.MakeContainer<T>(containerId);
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
