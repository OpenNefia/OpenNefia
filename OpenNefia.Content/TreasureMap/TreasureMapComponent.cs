using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.TreasureMap
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TreasureMapComponent : Component
    {
        /// <summary>
        /// Place the treasure is located.
        /// </summary>
        [DataField]
        public MapCoordinates TreasureCoords { get; set; }
    }
}