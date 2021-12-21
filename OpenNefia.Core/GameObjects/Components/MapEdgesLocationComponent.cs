using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects.Components
{
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
