using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Areas
{
    /// <summary>
    /// Keeps track of known entrances into maps contained by this area.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class AreaKnownEntrancesComponent : Component
    {
        /// <summary>
        /// { TargetGlobalArea -> { EntrancesLeadingToArea } }
        /// </summary>
        [DataField]
        public Dictionary<GlobalAreaId, Dictionary<EntityUid, AreaEntranceMetadata>> KnownEntrances { get; set; } = new();
    }

    [DataDefinition]
    public sealed class AreaEntranceMetadata
    {
        public AreaEntranceMetadata() {}

        public AreaEntranceMetadata(MapCoordinates coords, EntityUid entranceEntity)
        {
            MapCoordinates = coords;
            EntranceEntity = entranceEntity;
        }

        /// <summary>
        /// Location of this entrance, including its map ID.
        /// </summary>
        [DataField]
        public MapCoordinates MapCoordinates { get; set; }

        /// <summary>
        /// Entity UID of the map entrance.
        /// </summary>
        [DataField]
        public EntityUid EntranceEntity { get; set; }
    }
}