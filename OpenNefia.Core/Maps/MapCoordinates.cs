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
        public static readonly MapCoordinates Nullspace = new(MapId.Nullspace, Vector2i.Zero);

        /// <summary>
        ///     World Position coordinates.
        /// </summary>
        public readonly Vector2i Position;

        /// <summary>
        ///     ID of the map these coordinates reference.
        /// </summary>
        public readonly MapId MapId;

        /// <summary>
        ///     World position on the X axis.
        /// </summary>
        public int X => Position.X;

        /// <summary>
        ///     World position on the Y axis.
        /// </summary>
        public int Y => Position.Y;

        /// <summary>
        ///     Constructs a new instance of <c>MapCoordinates</c>.
        /// </summary>
        /// <param name="mapId">Map relevant to this position.</param>
        /// <param name="position">World position coordinates.</param>
        public MapCoordinates(MapId mapId, Vector2i position)
        {
            Position = position;
            MapId = mapId;
        }

        /// <summary>
        ///     Constructs a new instance of <c>MapCoordinates</c>.
        /// </summary>
        /// <param name="mapId">Map relevant to this position.</param>
        /// <param name="x">World position coordinate on the X axis.</param>
        /// <param name="y">World position coordinate on the Y axis.</param>
        public MapCoordinates(MapId mapId, int x, int y)
            : this(mapId, new Vector2i(x, y)) { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Map={MapId}, X={Position.X}, Y={Position.Y}";
        }

        /// <summary>
        ///     Checks that these coordinates are within a certain distance of another set.
        /// </summary>
        /// <param name="otherCoords">Other set of coordinates to use.</param>
        /// <param name="range">maximum distance between the two sets of coordinates.</param>
        /// <returns>True if the two points are within a given range.</returns>
        public bool InRange(MapCoordinates otherCoords, float range)
        {
            if (otherCoords.MapId != MapId)
            {
                return false;
            }

            return (otherCoords.Position - Position).LengthSquared < range * range;
        }

        /// <summary>
        ///     Tries to calculate the distance between two sets of coordinates.
        /// </summary>
        /// <param name="otherCoordinates"></param>
        /// <param name="distance"></param>
        /// <returns>True if it was possible to calculate the distance</returns>
        public bool TryDistance(MapCoordinates otherCoordinates, out float distance)
        {
            distance = 0f;

            if (MapId != otherCoordinates.MapId)
                return false;

            distance = (Position - otherCoordinates.Position).Length;
            return true;
        }

        public bool TryToEntity(IMapManager mapManager, out EntityCoordinates entityCoords)
        {
            if (!mapManager.TryGetMapEntity(MapId, out var entity))
            {
                entityCoords = EntityCoordinates.Invalid;
                return false;
            }

            entityCoords = new EntityCoordinates(entity.Uid, Position);
            return true;
        }

        /// <inheritdoc />
        public bool Equals(MapCoordinates other)
        {
            return Position.Equals(other.Position) && (MapId.Equals(other.MapId));
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
                return (Position.GetHashCode() * 397) ^ MapId.GetHashCode();
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
        /// <param name="position">World position coordinate.</param>
        /// <param name="mapId">Map relevant to this position.</param>
        public void Deconstruct(out Vector2i position, out MapId mapId)
        {
            mapId = MapId;
            position = Position;
        }

        /// <summary>
        /// Used to deconstruct this object into a tuple.
        /// </summary>
        /// <param name="x">World position coordinate on the X axis.</param>
        /// <param name="y">World position coordinate on the Y axis.</param>
        /// <param name="mapId">Map relevant to this position.</param>
        public void Deconstruct(out float x, out float y, out MapId mapId)
        {
            mapId = MapId;
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
            return new MapCoordinates(MapId, Position + offset);
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
    }
}
