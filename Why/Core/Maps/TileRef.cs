using Why.Core.Maths;

namespace Why.Core.Maps
{
    public struct TileRef
    {
        /// <summary>
        /// Map containing this tile.
        /// </summary>
        public readonly MapId MapId;

        /// <summary>
        /// Position of this tile in the map.
        /// </summary>
        public readonly Vector2i Position;

        /// <summary>
        /// The tile's data.
        /// </summary>
        public readonly Tile Tile;

        public TileRef(MapId mapId, int x, int y, Tile tile)
            : this(mapId, new Vector2i(x, y), tile)
        {
        }

        public TileRef(MapId mapId, Vector2i position, Tile tile)
        {
            this.MapId = mapId;
            this.Position = position;
            this.Tile = tile;
        }
    }
}