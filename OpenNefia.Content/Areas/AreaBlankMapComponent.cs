using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Areas
{
    /// <summary>
    /// Generates blank maps. Useful for unit testing.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaBlankMapComponent : Component
    {
        [DataField]
        public Vector2i Size { get; set; } = new(10, 10);

        [DataField]
        public PrototypeId<TilePrototype> Tile { get; set; } = Protos.Tile.Grass;
    }
}