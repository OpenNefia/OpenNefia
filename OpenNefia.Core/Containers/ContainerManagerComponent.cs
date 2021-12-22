using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Holds data about a set of entity containers on this entity.
    /// </summary>
    [ComponentReference(typeof(IContainerManager))]
    [RegisterComponent]
    public class ContainerManagerComponent : Component, IContainerManager, ISerializationHooks
    {
        [Dependency] private readonly IDynamicTypeFactoryInternal _dynFactory = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        [DataField]
        public Dictionary<string, IContainer> Containers = new();

        /// <inheritdoc />
        public sealed override string Name => "ContainerContainer";

        void ISerializationHooks.AfterDeserialization()
        {
            foreach (var (_, container) in Containers)
            {
                var baseContainer = (BaseContainer) container;
                baseContainer.Manager = this;
            }
        }

        /// <inheritdoc />
        protected override void OnRemove()
        {
            base.OnRemove();

            // IContainer.Shutdown modifies the _containers collection
            foreach (var container in Containers.Values.ToArray())
            {
                container.Shutdown();
            }

            Containers.Clear();
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            foreach (var container in Containers)
            {
                var baseContainer = (BaseContainer)container.Value;
                baseContainer.Manager = this;
                baseContainer.ID = container.Key;
            }
        }

        /// <inheritdoc />
        public T MakeContainer<T>(string id)
            where T : IContainer
        {
            return (T) MakeContainer(id, typeof(T));
        }

        /// <inheritdoc />
        public IContainer GetContainer(string id)
        {
            return Containers[id];
        }

        /// <inheritdoc />
        public bool HasContainer(string id)
        {
            return Containers.ContainsKey(id);
        }

        /// <inheritdoc />
        public bool TryGetContainer(string id, [NotNullWhen(true)] out IContainer? container)
        {
            var ret = Containers.TryGetValue(id, out var cont);
            container = cont!;
            return ret;
        }

        /// <inheritdoc />
        public bool TryGetContainer(EntityUid entity, [NotNullWhen(true)] out IContainer? container)
        {
            foreach (var contain in Containers.Values)
            {
                if (!contain.Deleted && contain.Contains(entity))
                {
                    container = contain;
                    return true;
                }
            }

            container = default;
            return false;
        }

        /// <inheritdoc />
        public bool ContainsEntity(EntityUid entity)
        {
            foreach (var container in Containers.Values)
            {
                if (!container.Deleted && container.Contains(entity)) return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void ForceRemove(EntityUid entity)
        {
            foreach (var container in Containers.Values)
            {
                if (container.Contains(entity)) container.ForceRemove(entity);
            }
        }

        /// <inheritdoc />
        public void InternalContainerShutdown(IContainer container)
        {
            Containers.Remove(container.ID);
        }

        /// <inheritdoc />
        public bool Remove(EntityUid entity)
        {
            foreach (var containers in Containers.Values)
            {
                if (containers.Contains(entity)) return containers.Remove(entity);
            }

            return true; // If we don't contain the entity, it will always be removed
        }

        /// <inheritdoc />
        protected override void Shutdown()
        {
            base.Shutdown();

            // On shutdown we won't get to process remove events in the containers so this has to be manually done.
            foreach (var container in Containers.Values)
            {
                foreach (var containerEntity in container.ContainedEntities)
                {
                    _entityManager.EventBus.RaiseEvent(EventSource.Local,
                        new UpdateContainerOcclusionEvent(containerEntity));
                }
            }
        }

        private IContainer MakeContainer(string id, Type type)
        {
            if (HasContainer(id)) throw new ArgumentException($"Container with specified ID already exists: '{id}'");

            var container = _dynFactory.CreateInstanceUnchecked<BaseContainer>(type);
            container.ID = id;
            container.Manager = this;

            Containers[id] = container;
            return container;
        }

        public AllContainersEnumerable GetAllContainers()
        {
            return new(this);
        }

        public readonly struct AllContainersEnumerable : IEnumerable<IContainer>
        {
            private readonly ContainerManagerComponent? _manager;

            public AllContainersEnumerable(ContainerManagerComponent? manager)
            {
                _manager = manager;
            }

            public AllContainersEnumerator GetEnumerator()
            {
                return new(_manager);
            }

            IEnumerator<IContainer> IEnumerable<IContainer>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public struct AllContainersEnumerator : IEnumerator<IContainer>
        {
            private Dictionary<string, IContainer>.ValueCollection.Enumerator _enumerator;

            public AllContainersEnumerator(ContainerManagerComponent? manager)
            {
                _enumerator = manager?.Containers.Values.GetEnumerator() ?? new();
                Current = default;
            }

            public bool MoveNext()
            {
                while (_enumerator.MoveNext())
                {
                    if (!_enumerator.Current.Deleted)
                    {
                        Current = _enumerator.Current;
                        return true;
                    }
                }

                return false;
            }

            void IEnumerator.Reset()
            {
                ((IEnumerator<IContainer>) _enumerator).Reset();
            }

            [AllowNull]
            public IContainer Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
    }
}
