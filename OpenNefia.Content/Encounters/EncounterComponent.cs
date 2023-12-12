using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Encounters
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterComponent : Component
    {
        [DataField]
        public int Level { get; set; } = 1;

        [DataField]
        public PrototypeId<TilePrototype> StoodWorldMapTile { get; set; } = Protos.Tile.WorldGrass;

        [DataField]
        public MapEntrance PreviousLocation { get; set; } = new();
    }
}