using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// General-purpose map entrance component, shared between world map entrances
    /// and entities like stairs.
    /// </summary>
    [Obsolete("After areas are implemented, replace with world map entrances/stairs with their own MapEntrance")]
    [RegisterComponent]
    public class MapEntranceComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapEntrance";

        /// <summary>
        /// Entrance to use.
        /// </summary>
        [DataField]
        public MapEntrance Entrance { get; set; } = new();

        /// <summary>
        /// Blueprint used for initializing this map.
        /// </summary>
        [DataField]
        public PrototypeId<MapPrototype> MapPrototype { get; set; }
    }

    [DataDefinition]
    public class MapEntrance
    {
        /// <summary>
        /// ID of the map this entrance leads to.
        /// </summary>
        /// <remarks>
        /// If this is null, the map will be generated from the 
        /// blueprint given by the <see cref="BlueprintFile"/>
        /// property. 
        /// </remarks>
        [DataField]
        public MapId? DestinationMapId { get; set; }

        /// <summary>
        /// Location in the destination map to place the player/allies.
        /// </summary>
        [DataField]
        public IMapStartLocation StartLocation { get; set; } = new CenterMapLocation();
    }
}
