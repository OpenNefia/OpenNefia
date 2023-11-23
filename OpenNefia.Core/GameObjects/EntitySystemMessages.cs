using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maps;

namespace OpenNefia.Core.GameObjects
{
    public sealed class EntityInitializedEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }

        public EntityInitializedEvent(EntityUid entityUid)
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
    /// The children of this entity are about to be deleted permanently, and will not be saved.
    /// </summary>
    [ByRefEvent]
    public sealed class BeforeEntityDeletedEvent : EntityEventArgs 
    {
    }

    /// <summary>
    /// The children of this entity are about to be unloaded, and will be kept in the save file.
    /// </summary>
    [ByRefEvent]
    public sealed class BeforeEntityUnloadedEvent : EntityEventArgs
    {
    }

    /// <summary>
    /// The children of this entity have been deleted, and the entity has been detached from the parent map/entity before deletion.
    /// </summary>
    [ByRefEvent]
    public sealed class EntityBeingDeletedEvent : EntityEventArgs
    {
    }

    /// <summary>
    /// The children of this entity have been unloaded, and the entity has been detached from the parent map/entity before unload.
    /// </summary>
    [ByRefEvent]
    public sealed class EntityBeingUnloadedEvent : EntityEventArgs
    {
    }
}
