using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Indicates a location that can be used by <see cref="MapEdgesLocation"/>
    /// to place a character in the map based on their facing direction.
    /// </summary>
    [RegisterComponent]
    public class MapEdgesLocationComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapEdgesLocation";

        /// <summary>
        /// Direction the player should enter the map from in the world map
        /// for them to be placed on this location.
        /// </summary>
        [DataField(required: true)]
        public Direction TargetDirection { get; set; }
    }
}
