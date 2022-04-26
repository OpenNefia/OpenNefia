using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;

namespace OpenNefia.Content.Maps
{
    [DataDefinition]
    public class MapEntrance
    {
        /// <summary>
        /// ID specifier of the map this entrance will lead to.
        /// </summary>
        [DataField("map", required: true)]
        public IMapIdSpecifier MapIdSpecifier { get; set; } = new NullMapIdSpecifier();

        /// <summary>
        /// Location in the destination map to place the player/allies.
        /// </summary>
        [DataField]
        public IMapStartLocation StartLocation { get; set; } = new CenterMapLocation();

        /// <summary>
        /// Generates a map entrace leading to the specified map coordinates.
        /// </summary>
        /// <param name="coords">Coordinates to move to when this entrance is entered.</param>
        public static MapEntrance FromMapCoordinates(MapCoordinates coords)
        {
            return new MapEntrance()
            {
                MapIdSpecifier = new BasicMapIdSpecifier(coords.MapId),
                StartLocation = new SpecificMapLocation(coords.Position)
            };
        }
    }
}
