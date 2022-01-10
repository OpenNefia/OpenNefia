using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Maps
{
    [DataDefinition]
    public class MapEntrance
    {
        /// <summary>
        /// ID specifier of the map this entrance will lead to.
        /// </summary>
        [DataField(required: true)]
        public IMapIdSpecifier MapIdSpecifier { get; set; } = new PrototypeMapIdSpecifier(Protos.MapProtos.Blank);

        /// <summary>
        /// Location in the destination map to place the player/allies.
        /// </summary>
        [DataField]
        public IMapStartLocation StartLocation { get; set; } = new CenterMapLocation();

    }
}
