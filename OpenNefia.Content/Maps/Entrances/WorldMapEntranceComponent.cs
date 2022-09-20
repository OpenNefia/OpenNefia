using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
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
