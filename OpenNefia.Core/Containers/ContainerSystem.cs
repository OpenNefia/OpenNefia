using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Containers
{
    public sealed partial class ContainerSystem : EntitySystem
    {
        [Dependency] private readonly IDynamicTypeFactoryInternal _dynFactory = default!;

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

            return (T)MakeContainer(uid, id, typeof(T), containerManager);
        }

        private IContainer MakeContainer(EntityUid uid, string id, Type type, ContainerManagerComponent containerManager)
        {
            if (HasContainer(uid, id, containerManager)) throw new ArgumentException($"Container with specified ID already exists: '{id}'");

            var container = _dynFactory.CreateInstanceUnchecked<BaseContainer>(type);
            container.ID = id;
            container.Manager = containerManager;

            containerManager.Containers[id] = container;
            return container;
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

            return containerManager.Containers[id];
        }

        /// <inheritdoc />
        public bool HasContainer(EntityUid uid, string id, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                return false;

            return containerManager.Containers.ContainsKey(id);
        }

        public bool TryGetContainer(EntityUid uid, string id, [NotNullWhen(true)] out IContainer? container, ContainerManagerComponent? containerManager = null)
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

        public void RemoveEntity(EntityUid uid, EntityUid containedUid, bool force = false, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager) || !EntityManager.EntityExists(containedUid))
                return;

            if (force)
                ForceRemove(containedUid, containerManager);
            else
                Remove(containedUid, containerManager);
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

        public ContainerManagerComponent.AllContainersEnumerable GetAllContainers(EntityUid uid, ContainerManagerComponent? containerManager = null)
        {
            if (!Resolve(uid, ref containerManager))
                return new ContainerManagerComponent.AllContainersEnumerable();

            return containerManager.GetAllContainers();
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
