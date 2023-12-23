using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Maps
{
    [DataDefinition]
    public class MapEntrance
    {
        public MapEntrance() {}

        public MapEntrance(IMapIdSpecifier mapIdSpecifier, IMapStartLocation startLocation)
        {
            MapIdSpecifier = mapIdSpecifier;
            StartLocation = startLocation;
        }

        /// <summary>
        /// ID specifier of the map this entrance will lead to.
        /// </summary>
        [DataField("map", required: true)]
        public IMapIdSpecifier MapIdSpecifier { get; set; } = new NullMapIdSpecifier();

        /// <summary>
        /// Location in the destination map to place the player/allies.
        /// By default it defers to the components in the map or area entity to pick the location.
        /// </summary>
        /// <seealso cref="MapOrAreaEntityStartLocation"/>
        [DataField]
        public IMapStartLocation StartLocation { get; set; } = new MapOrAreaEntityStartLocation();

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

        public bool TryGetMapCoordinates(EntityUid ent, IMap map, [NotNullWhen(true)]out MapCoordinates? result)
        {
            var mapID = MapIdSpecifier.GetMapId();
            if (mapID == null)
            {
                result = null;
                return false;
            }

            var position = StartLocation.GetStartPosition(ent, map);
            result = new MapCoordinates(mapID.Value, position);
            return true;
        }

        public override string ToString()
        {
            return $"{nameof(MapEntrance)}(mapId={MapIdSpecifier}, loc={StartLocation})";
        }
    }
}
