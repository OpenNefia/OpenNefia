using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Containers
{
    [UsedImplicitly]
    [SerializedType(ClassName)]
    public class ContainerSlot : BaseContainer
    {
        private const string ClassName = "ContainerSlot";

        /// <inheritdoc />
        public override IReadOnlyList<EntityUid> ContainedEntities
        {
            get
            {
                if (ContainedEntity == null)
                    return Array.Empty<EntityUid>();

                return _containedEntityArray;
            }
        }

        [DataField("ent")]
        public EntityUid? ContainedEntity
        {
            get => _containedEntity;
            private set
            {
                _containedEntity = value;
                if (value != null)
                    _containedEntityArray[0] = value!.Value;
            }
        }

        private EntityUid? _containedEntity;
        // Used by ContainedEntities to avoid allocating.
        private readonly EntityUid[] _containedEntityArray = new EntityUid[1];

        /// <inheritdoc />
        public override string ContainerType => ClassName;

        /// <inheritdoc />
        public override bool CanInsert(EntityUid toinsert, IEntityManager? entMan = null)
        {
            if (ContainedEntity != null)
                return false;
            return base.CanInsert(toinsert, entMan);
        }

        /// <inheritdoc />
        public override bool Contains(EntityUid contained)
        {
            if (contained == ContainedEntity)
                return true;
            return false;
        }

        /// <inheritdoc />
        protected override void InternalInsert(EntityUid toinsert, IEntityManager entMan)
        {
            ContainedEntity = toinsert;
            base.InternalInsert(toinsert, entMan);
        }

        /// <inheritdoc />
        protected override void InternalRemove(EntityUid toremove, IEntityManager entMan)
        {
            ContainedEntity = null;
            base.InternalRemove(toremove, entMan);
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            base.Shutdown();

            if (ContainedEntity is {} contained)
            {
                IoCManager.Resolve<IEntityManager>().DeleteEntity(contained);
            }
        }
    }
}
