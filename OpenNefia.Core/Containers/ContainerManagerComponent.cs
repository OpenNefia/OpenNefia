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
    public class ContainerManagerComponent : Component, IContainerManager, ISerializationHooks
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        [DataField("containers")]
        internal readonly Dictionary<ContainerId, IContainer> _containers = new();

        public IReadOnlyDictionary<ContainerId, IContainer> Containers => _containers;

        /// <inheritdoc />
        public sealed override string Name => "Containers";

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

            _containers.Clear();
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
        public void InternalContainerShutdown(IContainer container)
        {
            _containers.Remove(container.ID);
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
            private Dictionary<ContainerId, IContainer>.ValueCollection.Enumerator _enumerator;

            public AllContainersEnumerator(ContainerManagerComponent? manager)
            {
                _enumerator = manager?._containers.Values.GetEnumerator() ?? new();
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
