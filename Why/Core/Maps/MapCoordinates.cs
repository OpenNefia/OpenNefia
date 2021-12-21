﻿using System;
using JetBrains.Annotations;
using Love;
using Why.Core.Maths;

namespace Why.Core.Maps
{
    /// <summary>
    ///     Coordinates relative to a specific map.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public readonly struct MapCoordinates : IEquatable<MapCoordinates>
    {
        public static readonly MapCoordinates Nullspace = new(Vector2i.Zero, MapId.Nullspace);

        /// <summary>
        ///     World Position coordinates.
        /// </summary>
        public readonly Vector2i Position;

        /// <summary>
        ///     Map identifier relevant to this position.
        /// </summary>
        public readonly MapId MapId;

        /// <summary>
        ///     World position on the X axis.
        /// </summary>
        public float X => Position.X;

        /// <summary>
        ///     World position on the Y axis.
        /// </summary>
        public float Y => Position.Y;

        /// <summary>
        ///     Constructs a new instance of <c>MapCoordinates</c>.
        /// </summary>
        /// <param name="position">World position coordinates.</param>
        /// <param name="mapId">Map identifier relevant to this position.</param>
        public MapCoordinates(Vector2i position, MapId mapId)
        {
            Position = position;
            MapId = mapId;
        }

        /// <summary>
        ///     Constructs a new instance of <c>MapCoordinates</c>.
        /// </summary>
        /// <param name="x">World position coordinate on the X axis.</param>
        /// <param name="y">World position coordinate on the Y axis.</param>
        /// <param name="mapId">Map identifier relevant to this position.</param>
        public MapCoordinates(int x, int y, MapId mapId)
            : this(new Vector2i(x, y), mapId) { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Map={MapId}, X={Position.X:N2}, Y={Position.Y:N2}";
        }

        /// <inheritdoc />
        public bool Equals(MapCoordinates other)
        {
            return Position.Equals(other.Position) && MapId.Equals(other.MapId);
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
        public void Deconstruct(out MapId mapId, out float x, out float y)
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
            return new MapCoordinates(Position + offset, MapId);
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
