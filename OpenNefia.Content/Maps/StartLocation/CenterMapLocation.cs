using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Places the player at the center of the map.
    /// </summary>
    /// <remarks>
    /// According to <see cref="MapComponent"/>, this is the default.
    /// </remarks>
    public class CenterMapLocation : IMapStartLocation
    {
        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            return map.Size / 2;
        }
    }
}
