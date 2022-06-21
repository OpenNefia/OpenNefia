using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Map to travel to when exiting a map from the edges.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapEdgesEntranceComponent : Component
    {
        public override string Name => "MapEdgesEntrance";

        /// <summary>
        /// Entrance to use.
        /// </summary>
        [DataField]
        public MapEntrance Entrance { get; set; } = new();
    }
}
