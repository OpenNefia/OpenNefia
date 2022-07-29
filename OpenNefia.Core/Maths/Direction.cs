﻿using Love;
using System;

namespace OpenNefia.Core.Maths
{
    public enum Direction : sbyte
    {
        Invalid = -1,
        North = 0,
        NorthEast = 1,
        East = 2,
        SouthEast = 3,
        South = 4,
        SouthWest = 5,
        West = 6,
        NorthWest = 7,
        Center = 8,
    }

    [Flags]
    public enum DirectionFlag : sbyte
    {
        None = 0,
        South = 1 << 0,
        East = 1 << 1,
        North = 1 << 2,
        West = 1 << 3,
        Center = 1 << 4,

        SouthEast = South | East,
        NorthEast = North | East,
        NorthWest = North | West,
        SouthWest = South | West,
    }

    /// <summary>
    /// Extension methods for Direction enum.
    /// </summary>
    public static class DirectionExtensions
    {
        private const double Segment = 2 * Math.PI / 8.0; // Cut the circle into 8 pieces

        public static Direction AsDir(this DirectionFlag directionFlag)
        {
            switch (directionFlag)
            {
                case DirectionFlag.South:
                    return Direction.South;
                case DirectionFlag.SouthEast:
                    return Direction.SouthEast;
                case DirectionFlag.East:
                    return Direction.East;
                case DirectionFlag.NorthEast:
                    return Direction.NorthEast;
                case DirectionFlag.North:
                    return Direction.North;
                case DirectionFlag.NorthWest:
                    return Direction.NorthWest;
                case DirectionFlag.West:
                    return Direction.West;
                case DirectionFlag.SouthWest:
                    return Direction.SouthWest;
                case DirectionFlag.Center:
                    return Direction.Center;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DirectionFlag AsFlag(this Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return DirectionFlag.North;
                case Direction.NorthEast:
                    return DirectionFlag.NorthEast;
                case Direction.East:
                    return DirectionFlag.East;
                case Direction.SouthEast:
                    return DirectionFlag.SouthEast;
                case Direction.South:
                    return DirectionFlag.South;
                case Direction.SouthWest:
                    return DirectionFlag.SouthWest;
                case Direction.West:
                    return DirectionFlag.West;
                case Direction.NorthWest:
                    return DirectionFlag.NorthWest;
                case Direction.Center:
                    return DirectionFlag.Center;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Converts a direction vector to the closest Direction enum.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Direction GetDir(this Vector2 vec)
        {
            return Angle.FromWorldVec(vec).GetDir();
        }

        /// <summary>
        /// Converts a direction vector to the closest Direction enum.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Direction GetDir(this Vector2i vec)
        {
            return Angle.FromWorldVec(vec).GetDir();
        }

        /// <summary>
        /// Converts a direction vector to the closest cardinal Direction enum.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Direction GetCardinalDir(this Vector2i vec)
        {
            return Angle.FromWorldVec(vec).GetCardinalDir();
        }

        public static bool IsCardinal(this Direction direction)
        {
            return direction.ToIntVec().GetCardinalDir() == direction;
        }

        public static Direction GetOpposite(this Direction direction)
        {
            return direction switch
            {
                Direction.East => Direction.West,
                Direction.West => Direction.East,
                Direction.South => Direction.North,
                Direction.North => Direction.South,
                Direction.SouthEast => Direction.NorthWest,
                Direction.NorthWest => Direction.SouthEast,
                Direction.SouthWest => Direction.NorthEast,
                Direction.NorthEast => Direction.SouthWest,
                Direction.Center => Direction.Center,
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        public static Direction GetClockwise90Degrees(this Direction direction)
        {
            return direction switch
            {
                Direction.East => Direction.North,
                Direction.West => Direction.South,
                Direction.South => Direction.East,
                Direction.North => Direction.West,
                Direction.SouthEast => Direction.NorthEast,
                Direction.NorthWest => Direction.SouthWest,
                Direction.SouthWest => Direction.SouthEast,
                Direction.NorthEast => Direction.NorthWest,
                Direction.Center => Direction.Center,
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        /// <summary>
        /// Converts a direction to an angle, where angle is -PI to +PI.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Angle ToAngle(this Direction dir)
        {
            if (dir == Direction.Center)
                return 0.0;

            var ang = Segment * (int)dir;

            if (ang > Math.PI) // convert 0 > 2PI to -PI > +PI
                ang -= 2 * Math.PI;

            return ang;
        }

        private static readonly Vector2[] DirectionVectors = {
            new (0, -1),
            new Vector2(1, -1).Normalized,
            new (1, 0),
            new Vector2(1, 1).Normalized,
            new (0, 1),
            new Vector2(-1, 1).Normalized,
            new (-1, 0),
            new Vector2(-1, -1).Normalized,
            new (0, 0)
        };

        private static readonly Vector2i[] IntDirectionVectors = {
            new (0, -1),
            new (1, -1),
            new (1, 0),
            new (1, 1),
            new (0, 1),
            new (-1, 1),
            new (-1, 0),
            new (-1, -1),
            new (0, 0)
        };

        /// <summary>
        /// Converts a Direction to a normalized Direction vector.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>a normalized 2D Vector</returns>
        /// <exception cref="IndexOutOfRangeException">if invalid Direction is used</exception>
        /// <seealso cref="Vector2"/>
        public static Vector2 ToVec(this Direction dir)
        {
            return DirectionVectors[(int)dir];
        }

        /// <summary>
        /// Converts a Direction to a Vector2i. Useful for getting adjacent tiles.
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <returns>an 2D int Vector</returns>
        /// <exception cref="IndexOutOfRangeException">if invalid Direction is used</exception>
        /// <seealso cref="Vector2i"/>
        public static Vector2i ToIntVec(this Direction dir)
        {
            return IntDirectionVectors[(int)dir];
        }

        /// <summary>
        ///     Offset 2D integer vector by a given direction.
        ///     Convenience for adding <see cref="ToIntVec"/> to <see cref="Vector2i"/>
        /// </summary>
        /// <param name="vec">2D integer vector</param>
        /// <param name="dir">Direction by which we offset</param>
        /// <returns>a newly vector offset by the <param name="dir">dir</param> or exception if the direction is invalid</returns>
        public static Vector2i Offset(this Vector2i vec, Direction dir)
        {
            return vec + dir.ToIntVec();
        }

        /// <summary>
        /// Converts a direction vector to an angle, where angle is -PI to +PI.
        /// </summary>
        /// <param name="vec">Vector to get the angle from.</param>
        /// <returns>Angle of the vector.</returns>
        public static Angle ToAngle(this Vector2 vec)
        {
            return new(vec);
        }

        public static Angle ToWorldAngle(this Vector2 vec)
        {
            return Angle.FromWorldVec(vec);
        }
    }
}