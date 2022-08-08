using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Containers
{
    public sealed partial class ContainerSystem : EntitySystem
    {
        /// <inheritdoc />
        public bool TryGetContainerMan(EntityUid entity, [NotNullWhen(true)] out ContainerManagerComponent? manager)
        {
            DebugTools.Assert(EntityManager.EntityExists(entity));

            var parentTransform = EntityManager.GetComponent<SpatialComponent>(entity).Parent;
            if (parentTransform != null && TryGetManagerComp(parentTransform.Owner, out manager)
                && ContainsEntity(manager.Owner, entity, manager))
                return true;

            manager = default;
            return false;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool TryRemoveFromContainer(EntityUid entity, bool force = false)
        {
            return TryRemoveFromContainer(entity, force, out _);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void CleanContainer(IContainer container)
        {
            foreach (var ent in container.ContainedEntities.ToArray())
            {
                if (EntityManager.Deleted(ent)) continue;
                container.ForceRemove(ent);
                EntityManager.DeleteEntity(ent);
            }
        }

        /// <inheritdoc />
        public void AttachParentToContainerOrMap(SpatialComponent transform)
        {
            if (transform.Parent == null
                || !TryGetContainingContainer(transform.Parent.Owner, out var container)
                || !TryInsertIntoContainer(transform, container))
                transform.AttachToMap();
        }

        private bool TryInsertIntoContainer(SpatialComponent transform, IContainer container)
        {
            if (container.Insert(transform.Owner)) return true;

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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public T EnsureContainer<T>(EntityUid entity, ContainerId containerId)
            where T : IContainer
        {
            return EnsureContainer<T>(entity, containerId, out _);
        }

        /// <inheritdoc />
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
