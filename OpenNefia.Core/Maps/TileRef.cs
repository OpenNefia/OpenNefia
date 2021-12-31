using System;
using JetBrains.Annotations;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    ///     All of the information needed to reference a tile in the game.
    /// </summary>
    [PublicAPI]
    public readonly struct TileRef : IEquatable<TileRef>
    {
        public static TileRef Empty => new(MapId.Nullspace, Vector2i.Zero, Tile.Empty);

        /// <summary>
        ///     Identifier of the <see cref="IMap"/> this Tile belongs to.
        /// </summary>
        public readonly MapId MapId;

        /// <summary>
        ///     Positional indices of this tile on the grid.
        /// </summary>
        public readonly Vector2i Position;

        /// <summary>
        ///     Actual data of this Tile.
        /// </summary>
        public readonly Tile Tile;

        /// <summary>
        ///     Constructs a new instance of TileRef.
        /// </summary>
        /// <param name="mapId">Identifier of the map this tile belongs to.</param>
        /// <param name="xIndex">Positional X index of this tile on the grid.</param>
        /// <param name="yIndex">Positional Y index of this tile on the grid.</param>
        /// <param name="tile">Actual data of this tile.</param>
        internal TileRef(MapId mapId, int xIndex, int yIndex, Tile tile)
            : this(mapId, new Vector2i(xIndex, yIndex), tile) { }

        /// <summary>
        ///     Constructs a new instance of TileRef.
        /// </summary>
        /// <param name="mapId">Identifier of the map this tile belongs to.</param>
        /// <param name="mapPosition">Positional indices of this tile on the grid.</param>
        /// <param name="tile">Actual data of this tile.</param>
        internal TileRef(MapId mapId, Vector2i mapPosition, Tile tile)
        {
            MapId = mapId;
            Position = mapPosition;
            Tile = tile;
        }

        /// <summary>
        ///     Grid index on the X axis.
        /// </summary>
        public int X => Position.X;

        /// <summary>
        ///     Grid index on the Y axis.
        /// </summary>
        public int Y => Position.Y;

        /// <summary>
        ///     Coordinates in the map.
        /// </summary>
        public MapCoordinates MapPosition => new(MapId, Position);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"TileRef: {X},{Y} ({Tile})";
        }

        /// <inheritdoc />
        public bool Equals(TileRef other)
        {
            return MapId.Equals(other.MapId)
                   && Position.Equals(other.Position)
                   && Tile.Equals(other.Tile);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is TileRef other && Equals(other);
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(TileRef a, TileRef b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(TileRef a, TileRef b)
        {
            return !a.Equals(b);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MapId.GetHashCode();
                hashCode = hashCode * 397 ^ Position.GetHashCode();
                hashCode = hashCode * 397 ^ Tile.GetHashCode();
                return hashCode;
            }
        }
    }
}
