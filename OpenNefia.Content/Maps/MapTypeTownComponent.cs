using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class MapTypeTownComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeTown";

        [DataField]
        public int? TownId { get; set; }
    }
}
