using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Areas;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// This entity should be placed on a world map and leads to another (non-world) map.
    /// If the area associated with the entrance has a global area ID, the 
    /// <see cref="AreaKnownEntrancesSystem"/> will register the position of this entity,
    /// so that it can properly generate delivery quests.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public class WorldMapEntranceComponent : Component
    {
        /// <summary>
        /// Entrance to use.
        /// </summary>
        [DataField]
        public MapEntrance Entrance { get; set; } = new();
    }
}
