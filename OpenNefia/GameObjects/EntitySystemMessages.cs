using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maps;

namespace OpenNefia.Core.GameObjects
{
    public sealed class EntityInitializedMessage : EntityEventArgs
    {
        public Entity Entity { get; }

        public EntityInitializedMessage(Entity entity)
        {
            Entity = entity;
        }
    }

    public class EntMapIdChangedMessage : EntityEventArgs
    {
        public EntMapIdChangedMessage(Entity entity, MapId oldMapId)
        {
            Entity = entity;
            OldMapId = oldMapId;
        }

        public Entity Entity { get; }
        public MapId OldMapId { get; }
    }

    /// <summary>
    /// The children of this entity are about to be deleted.
    /// </summary>
    public sealed class EntityTerminatingEvent : EntityEventArgs { }

    public sealed class EntityDeletedMessage : EntityEventArgs
    {
        public Entity Entity { get; }

        public EntityDeletedMessage(Entity entity)
        {
            Entity = entity;
        }
    }
}
