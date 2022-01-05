using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Places the player at a specific location in the map.
    /// </summary>
    public class SpecificMapLocation : IMapStartLocation
    {
        [DataField("pos")]
        public Vector2i Position { get; }

        public SpecificMapLocation() { }

        public SpecificMapLocation(Vector2i position)
        {
            Position = position;
        }

        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            return Position;
        }
    }
}
