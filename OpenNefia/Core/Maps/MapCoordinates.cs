using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Love;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    ///     Coordinates relative to a specific map.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public readonly struct MapCoordinates : IEquatable<MapCoordinates>
    {
        public static readonly MapCoordinates Nullspace = new(null, Vector2i.Zero);

        /// <summary>
        ///     World Position coordinates.
        /// </summary>
        public readonly Vector2i Position;

        /// <summary>
        ///     Map these coordinates reference.
        /// </summary>
        public readonly IMap? Map;

        /// <summary>
        ///     World position on the X axis.
        /// </summary>
        public int X => Position.X;

        /// <summary>
        ///     World position on the Y axis.
        /// </summary>
        public int Y => Position.Y;

        /// <summary>
        ///     Tile this position references.
        /// </summary>
        public Tile? GetTile()
        {
            if (!IsInBounds())
                return null;

            return Map.Tiles[X, Y];
        }

        /// <summary>
        ///     Tile memory this position references.
        /// </summary>
        public Tile? GetTileMemory()
        {
            if (!IsInBounds())
                return null;

            return Map.TileMemory[X, Y];
        }

        /// <summary>
        ///     Constructs a new instance of <c>MapCoordinates</c>.
        /// </summary>
        /// <param name="mapId">Map relevant to this position.</param>
        /// <param name="position">World position coordinates.</param>
        public MapCoordinates(IMap? map, Vector2i position)
        {
            Position = position;
            Map = map;
        }

        /// <summary>
        ///     Constructs a new instance of <c>MapCoordinates</c>.
        /// </summary>
        /// <param name="mapId">Map relevant to this position.</param>
        /// <param name="x">World position coordinate on the X axis.</param>
        /// <param name="y">World position coordinate on the Y axis.</param>
        public MapCoordinates(IMap? map, int x, int y)
            : this(map, new Vector2i(x, y)) { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Map={Map?.Id}, X={Position.X}, Y={Position.Y}";
        }

        /// <inheritdoc />
        public bool Equals(MapCoordinates other)
        {
            return Position.Equals(other.Position) && (Map?.Id.Equals(other.Map?.Id) ?? false);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is MapCoordinates other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ (Map?.Id ?? MapId.Nullspace).GetHashCode();
            }
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(MapCoordinates a, MapCoordinates b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(MapCoordinates a, MapCoordinates b)
        {
            return !a.Equals(b);
        }


        /// <summary>
        /// Used to deconstruct this object into a tuple.
        /// </summary>
        /// <param name="x">World position coordinate on the X axis.</param>
        /// <param name="y">World position coordinate on the Y axis.</param>
        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Used to deconstruct this object into a tuple.
        /// </summary>
        /// <param name="mapId">Map identifier relevant to this position.</param>
        /// <param name="x">World position coordinate on the X axis.</param>
        /// <param name="y">World position coordinate on the Y axis.</param>
        public void Deconstruct(out IMap? map, out float x, out float y)
        {
            map = Map;
            x = X;
            y = Y;
        }

        /// <summary>
        /// Used to get a copy of the coordinates with an offset.
        /// </summary>
        /// <param name="offset">Offset to apply to these coordinates</param>
        /// <returns>A copy of these coordinates, but offset.</returns>
        public MapCoordinates Offset(Vector2i offset)
        {
            return new MapCoordinates(Map, Position + offset);
        }

        /// <summary>
        /// Used to get a copy of the coordinates with an offset.
        /// </summary>
        /// <param name="x">X axis offset to apply to these coordinates</param>
        /// <param name="y">Y axis offset to apply to these coordinates</param>
        /// <returns>A copy of these coordinates, but offset.</returns>
        public MapCoordinates Offset(int x, int y)
        {
            return Offset(new Vector2i(x, y));
        }

        [MemberNotNullWhen(true, nameof(Map))]
        public bool IsInBounds()
        {
            if (Map == null)
                return false;

            return X >= 0 && Y >= 0 && X < Map.Width && Y < Map.Height;
        }
    }
}
