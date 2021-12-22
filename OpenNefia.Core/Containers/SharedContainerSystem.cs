using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Containers
{
    public abstract class SharedContainerSystem : EntitySystem
    {
        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EntityParentChangedEvent>(HandleParentChanged, nameof(HandleParentChanged));
        }

        // TODO: Make ContainerManagerComponent ECS and make these proxy methods the real deal.

        #region Proxy Methods

        public T MakeContainer<T>(EntityUid uid, string id, ContainerManagerComponent? containerManager = null)
            where T : IContainer
        {
            if (!Resolve(uid, ref containerManager, false))
                containerManager = EntityManager.AddComponent<ContainerManagerComponent>(uid); // Happy Vera.

            return containerManager.MakeContainer<T>(id);
        }

        public T EnsureContainer<T>(EntityUid uid, string id, ContainerManagerComponent? containerManager = null)
            where T : IContainer
        {
            if (!Resolve(uid, ref containerManager, false))
                containerManager = EntityManager.AddComponent<ContainerManagerComponent>(uid);

            if (TryGetContainer(uid, id, out var container, containerManager))
                return (T)container;

            return MakeContainer<T>(uid, id, containerManager);
        }

        public IContainer GetContainer(EntityUid uid, string id, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                throw new ArgumentException("Entity does not have a ContainerManagerComponent!", nameof(uid));

            return containerManager.GetContainer(id);
        }

        public bool HasContainer(EntityUid uid, string id, ContainerManagerComponent? containerManager)
        {
            if (!Resolve(uid, ref containerManager, false))
                return false;

            return containerManager.HasContainer(id);
        }

        public bool TryGetContainer(EntityUid uid, string id, [NotNullWhen(true)] out IContainer? container, ContainerManagerComponent? containerManager = null)
        {
            if (Resolve(uid, ref containerManager, false))
                return containerManager.TryGetContainer(id, out container);

            container = null;
            return false;
        }

        public bool TryGetContainingContainer(EntityUid uid, EntityUid containedUid, [NotNullWhen(true)] out IContainer? container, ContainerManagerComponent? containerManager = null)
        {
            if (Resolve(uid, ref containerManager, false) && EntityManager.EntityExists(containedUid))
                return containerManager.TryGetContainer(containedUid, out container);

            container = null;
            return false;
        }

        public bool ContainsEntity(EntityUid uid, EntityUid containedUid, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager, false) || !EntityManager.EntityExists(containedUid))
                return false;

            return containerManager.ContainsEntity(containedUid);
        }

        public void RemoveEntity(EntityUid uid, EntityUid containedUid, bool force = false, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager) || !EntityManager.EntityExists(containedUid))
                return;

            if (force)
                containerManager.ForceRemove(containedUid);
            else
                containerManager.Remove(containedUid);
        }

        public ContainerManagerComponent.AllContainersEnumerable GetAllContainers(EntityUid uid, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                return new ContainerManagerComponent.AllContainersEnumerable();

            return containerManager.GetAllContainers();
        }

        #endregion

        #region Container Helpers

        public bool TryGetContainingContainer(EntityUid uid, [NotNullWhen(true)] out IContainer? container, SpatialComponent? transform = null)
        {
            container = null;
            if (!Resolve(uid, ref transform, false))
                return false;

            if (!transform.ParentUid.IsValid())
                return false;

            return TryGetContainingContainer(transform.ParentUid, uid, out container);
        }

        public bool IsEntityInContainer(EntityUid uid, SpatialComponent? transform = null)
        {
            return TryGetContainingContainer(uid, out _, transform);
        }

        #endregion

        // Eject entities from their parent container if the parent change is done by the transform only.
        private void HandleParentChanged(ref EntityParentChangedEvent message)
        {
            var oldParentEntity = message.OldParent;

            if (oldParentEntity == null || !EntityManager.EntityExists(oldParentEntity!.Value))
                return;

            if (EntityManager.TryGetComponent(oldParentEntity!.Value, out IContainerManager? containerManager))
                containerManager.ForceRemove(message.EntityUid);
        }
    }
}
