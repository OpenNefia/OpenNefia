﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenNefia.Core.Maths
{
    /// <summary>
    ///     Axis Aligned rectangular box in screen coordinates.
    ///     Uses a left-handed coordinate system. This means that X+ is to the right and Y+ down.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct UIBox2 : IEquatable<UIBox2>
    {
        /// <summary>
        ///     The X coordinate of the left edge of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 0)] public float Left;

        /// <summary>
        ///     The Y coordinate of the top edge of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 1)] public float Top;

        /// <summary>
        ///     The X coordinate of the right edge of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 2)] public float Right;

        /// <summary>
        ///     The Y coordinate of the bottom of the box.
        /// </summary>
        [FieldOffset(sizeof(float) * 3)] public float Bottom;

        [FieldOffset(sizeof(float) * 0)] public Vector2 TopLeft;
        [FieldOffset(sizeof(float) * 2)] public Vector2 BottomRight;

        public readonly Vector2 TopRight => new(Right, Top);
        public readonly Vector2 BottomLeft => new(Left, Bottom);
        public readonly float Width => MathF.Abs(Right - Left);
        public readonly float Height => MathF.Abs(Top - Bottom);
        public readonly Vector2 Size => new(Width, Height);
        public readonly Vector2 Center => TopLeft + Size / 2;

        public UIBox2(Vector2 leftTop, Vector2 rightBottom)
        {
            Unsafe.SkipInit(out this);

            TopLeft = leftTop;
            BottomRight = rightBottom;
        }

        public UIBox2(float left, float top, float right, float bottom)
        {
            Unsafe.SkipInit(out this);

            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public static UIBox2 FromDimensions(float left, float top, float width, float height)
        {
            return new(left, top, left + width, top + height);
        }

        public static UIBox2 FromDimensions(Vector2 leftTopPosition, Vector2 size)
        {
            return FromDimensions(leftTopPosition.X, leftTopPosition.Y, size.X, size.Y);
        }

        public readonly bool Intersects(UIBox2 other)
        {
            return other.Bottom >= Top && other.Top <= Bottom && other.Right >= Left &&
                   other.Left <= Right;
        }

        public readonly bool IsEmpty()
        {
            return MathHelper.CloseToPercent(Width, 0.0f) && MathHelper.CloseToPercent(Height, 0.0f);
        }

        public readonly bool Encloses(UIBox2 inner)
        {
            return Left < inner.Left && Bottom > inner.Bottom && Right > inner.Right &&
                   Top < inner.Top;
        }

        public readonly bool Contains(float x, float y)
        {
            return Contains(new Vector2(x, y));
        }

        public readonly bool Contains(Vector2 point, bool closedRegion = true)
        {
            var xOk = closedRegion
                ? point.X >= Left ^ point.X > Right
                : point.X > Left ^ point.X >= Right;
            var yOk = closedRegion
                ? point.Y >= Top ^ point.Y > Bottom
                : point.Y > Top ^ point.Y >= Bottom;
            return xOk && yOk;
        }

        /// <summary>
        ///     Uniformly scales the box by a given scalar.
        ///     This scaling is done such that the center of the resulting box is the same as this box.
        ///     i.e. it scales around the center of the box, just changing width/height.
        /// </summary>
        /// <param name="scalar">Value to scale the box by.</param>
        /// <returns>Scaled box.</returns>
        public readonly UIBox2 Scale(float scalar)
        {
            if (scalar < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scalar), scalar, "Scalar cannot be negative.");
            }

            var center = Center;
            var halfSize = Size / 2 * scalar;
            return new UIBox2(
                center - halfSize,
                center + halfSize);
        }

        /// <summary>Returns a UIBox2 translated by the given amount.</summary>
        public readonly UIBox2 Translated(Vector2 point)
        {
            return new(Left + point.X, Top + point.Y, Right + point.X, Bottom + point.Y);
        }

        /// <summary>
        ///     Calculates the "intersection" of this and another box.
        ///     Basically, the smallest region that fits in both boxes.
        /// </summary>
        /// <param name="other">The box to calculate the intersection with.</param>
        /// <returns>
        ///     <c>null</c> if there is no intersection, otherwise the smallest region that fits in both boxes.
        /// </returns>
        public readonly UIBox2? Intersection(in UIBox2 other)
        {
            if (!Intersects(other))
            {
                return null;
            }

            return new UIBox2(
                Vector2.ComponentMax(TopLeft, other.TopLeft),
                Vector2.ComponentMin(BottomRight, other.BottomRight));
        }

        public readonly bool Equals(UIBox2 other)
        {
            return Left.Equals(other.Left) && Right.Equals(other.Right) && Top.Equals(other.Top) &&
                   Bottom.Equals(other.Bottom);
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is null) return false;
            return obj is UIBox2 box2 && Equals(box2);
        }

        public override readonly int GetHashCode()
        {
            unchecked
            {
                var hashCode = Left.GetHashCode();
                hashCode = hashCode * 397 ^ Right.GetHashCode();
                hashCode = hashCode * 397 ^ Top.GetHashCode();
                hashCode = hashCode * 397 ^ Bottom.GetHashCode();
                return hashCode;
            }
        }

        public static explicit operator UIBox2i(UIBox2 box)
        {
            return new((int)box.Left, (int)box.Top, (int)box.Right, (int)box.Bottom);
        }

        public static implicit operator Love.RectangleF(UIBox2 box)
        {
            return new(box.Left, box.Top, box.Width, box.Height);
        }

        /// <summary>
        ///     Compares two objects for equality by value.
        /// </summary>
        public static bool operator ==(UIBox2 a, UIBox2 b)
        {
            return MathHelper.CloseToPercent(a.Bottom, b.Bottom) &&
                   MathHelper.CloseToPercent(a.Right, b.Right) &&
                   MathHelper.CloseToPercent(a.Top, b.Top) &&
                   MathHelper.CloseToPercent(a.Left, b.Left);
        }

        public static bool operator !=(UIBox2 a, UIBox2 b)
        {
            return !(a == b);
        }

        public static UIBox2 operator +(UIBox2 box, (float lo, float to, float ro, float bo) offsets)
        {
            var (lo, to, ro, bo) = offsets;

            return new UIBox2(box.Left + lo, box.Top + to, box.Right + ro, box.Bottom + bo);
        }

        public override readonly string ToString()
        {
            return $"({Left}, {Top}, {Right}, {Bottom})";
        }
    }
}
