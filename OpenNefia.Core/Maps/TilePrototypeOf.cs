using OpenNefia.Core.Prototypes;
using TilePrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Maps.TilePrototype>;

namespace OpenNefia.Core.Maps
{
    public static class TilePrototypeOf
    {
        /// <summary>
        /// This is the tile prototype that should correspond to 
        /// <see cref="Tile.Empty" />, i.e. it should be registered
        /// first so it gets tile index 0.
        /// </summary>
        public static TilePrototypeId Empty = new(nameof(Empty));
    }
}
