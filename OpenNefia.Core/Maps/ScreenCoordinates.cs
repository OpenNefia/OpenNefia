using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
using System;
using JetBrains.Annotations;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    ///     Contains the coordinates of a position on the rendering screen.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public readonly struct ScreenCoordinates : IEquatable<ScreenCoordinates>
    {
        /// <summary>
        ///     Position on the rendering screen.
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        ///     Screen position on the X axis.
        /// </summary>
        public float X => Position.X;

        /// <summary>
        ///     Screen position on the Y axis.
        /// </summary>
        public float Y => Position.Y;

        /// <summary>
        ///     Constructs a new instance of <c>ScreenCoordinates</c>.
        /// </summary>
        /// <param name="position">Position on the rendering screen.</param>
        /// <param name="window">Window for the coordinates.</param>
        public ScreenCoordinates(Vector2 position)
        {
            Position = position;
        }

        /// <summary>
        ///     Constructs a new instance of <c>ScreenCoordinates</c>.
        /// </summary>
        /// <param name="x">X axis of a position on the screen.</param>
        /// <param name="y">Y axis of a position on the screen.</param>
        /// <param name="window">Window for the coordinates.</param>
        public ScreenCoordinates(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Position.X}, {Position.Y})";
        }

        /// <inheritdoc />
        public bool Equals(ScreenCoordinates other)
        {
            return Position.Equals(other.Position);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is ScreenCoordinates other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Position);
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(ScreenCoordinates a, ScreenCoordinates b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(ScreenCoordinates a, ScreenCoordinates b)
        {
            return !a.Equals(b);
        }
    }
}
