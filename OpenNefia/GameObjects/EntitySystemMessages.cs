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
        public EntityUid EntityUid { get; }

        public EntityInitializedMessage(EntityUid entityUid)
        {
            EntityUid = entityUid;
        }
    }

    public class EntMapIdChangedEvent : EntityEventArgs
    {
        public EntMapIdChangedEvent(EntityUid entityUid, MapId oldMapId)
        {
            EntityUid = entityUid;
            OldMapId = oldMapId;
        }

        public EntityUid EntityUid { get; }
        public MapId OldMapId { get; }
    }

    /// <summary>
    /// The children of this entity are about to be deleted.
    /// </summary>
    public sealed class EntityTerminatingEvent : EntityEventArgs 
    {
    }

    public sealed class EntityDeletedMessage : EntityEventArgs
    {
        public EntityUid EntityUid { get; }

        public EntityDeletedMessage(EntityUid entityUid)
        {
            EntityUid = entityUid;
        }
    }
}
