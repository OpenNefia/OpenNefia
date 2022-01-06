using System.Collections.Generic;
using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Default implementation for containers,
    /// cannot be inherited. If additional logic is needed,
    /// this logic should go on the systems that are holding this container.
    /// For example, inventory containers should be modified only through an inventory component.
    /// </summary>
    [UsedImplicitly]
    [SerializedType(ClassName)]
    public sealed class Container : BaseContainer
    {
        private const string ClassName = "Container";

        /// <summary>
        /// The generic container class uses a list of entities
        /// </summary>
        [DataField("ents")]
        private readonly List<EntityUid> _containerList = new();

        /// <inheritdoc />
        public override IReadOnlyList<EntityUid> ContainedEntities => _containerList;

        /// <inheritdoc />
        public override string ContainerType => ClassName;

        /// <inheritdoc />
        protected override void InternalInsert(EntityUid toinsert, IEntityManager entMan)
        {
            _containerList.Add(toinsert);
            base.InternalInsert(toinsert, entMan);
        }

        /// <inheritdoc />
        protected override void InternalRemove(EntityUid toremove, IEntityManager entMan)
        {
            _containerList.Remove(toremove);
            base.InternalRemove(toremove, entMan);
        }

        /// <inheritdoc />
        public override bool Contains(EntityUid contained)
        {
            return _containerList.Contains(contained);
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            base.Shutdown();

            var entMan = IoCManager.Resolve<IEntityManager>();
            foreach (var entity in _containerList)
            {
                entMan.DeleteEntity(entity);
            }
        }
    }
}
