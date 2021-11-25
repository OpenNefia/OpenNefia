using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Maps;

namespace Why.Core.GameObjects
{
    public sealed class EntityInitializedMessage : EntityEventArgs
    {
        public IEntity Entity { get; }

        public EntityInitializedMessage(IEntity entity)
        {
            Entity = entity;
        }
    }

    public class EntMapIdChangedMessage : EntityEventArgs
    {
        public EntMapIdChangedMessage(IEntity entity, MapId oldMapId)
        {
            Entity = entity;
            OldMapId = oldMapId;
        }

        public IEntity Entity { get; }
        public MapId OldMapId { get; }
    }

    /// <summary>
    /// The children of this entity are about to be deleted.
    /// </summary>
    public sealed class EntityTerminatingEvent : EntityEventArgs { }

    public sealed class EntityDeletedMessage : EntityEventArgs
    {
        public IEntity Entity { get; }

        public EntityDeletedMessage(IEntity entity)
        {
            Entity = entity;
        }
    }
}
