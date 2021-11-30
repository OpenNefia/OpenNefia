using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenNefia.Core.Maths
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Box2i : IEquatable<Box2i>
    {
        [FieldOffset(sizeof(int) * 0)] public int Left;
        [FieldOffset(sizeof(int) * 1)] public int Top;
        [FieldOffset(sizeof(int) * 2)] public int Right;
        [FieldOffset(sizeof(int) * 3)] public int Bottom;

        [FieldOffset(sizeof(int) * 0)] public Vector2i TopLeft;
        [FieldOffset(sizeof(int) * 2)] public Vector2i BottomRight;

        public readonly Vector2i TopRight => new(Right, Top);
        public readonly Vector2i BottomLeft => new(Left, Bottom);
        public readonly int Width => Math.Abs(Right - Left);
        public readonly int Height => Math.Abs(Top - Bottom);
        public readonly Vector2i Size => new(Width, Height);

        public readonly int Area => Width * Height;

        public Box2i(Vector2i topLeft, Vector2i bottomRight)
        {
            Unsafe.SkipInit(out this);

            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public Box2i(int left, int top, int right, int bottom)
        {
            Unsafe.SkipInit(out this);

            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static Box2i FromDimensions(int left, int top, int width, int height)
        {
            return new(left, top, left + width, top + height);
        }

        public static Box2i FromDimensions(Vector2i position, Vector2i size)
        {
            return FromDimensions(position.X, position.Y, size.X, size.Y);
        }

        public readonly bool Contains(int x, int y)
        {
            return Contains(new Vector2i(x, y));
        }

        public readonly bool Contains(Vector2i point, bool closedRegion = true)
        {
            var xOk = closedRegion
                ? point.X >= Left ^ point.X > Right
                : point.X > Left ^ point.X >= Right;
            var yOk = closedRegion
                ? point.Y >= Bottom ^ point.Y > Top
                : point.Y > Bottom ^ point.Y >= Top;
            return xOk && yOk;
        }

        public readonly bool IsEmpty()
        {
            return Bottom == Top || Left == Right;
        }

        /// <summary>Returns a UIBox2 translated by the given amount.</summary>
        public readonly Box2i Translated(Vector2i point)
        {
            return new(Left + point.X, Top + point.Y, Right + point.X, Bottom + point.Y);
        }

        /// <summary>
        ///     Returns the smallest rectangle that contains both of the rectangles.
        /// </summary>
        public readonly Box2i Union(in Box2i other)
        {
            var left = Math.Min(Left, other.Left);
            var right = Math.Max(Right, other.Right);
            var bottom = Math.Min(Bottom, other.Bottom);
            var top = Math.Max(Top, other.Top);

            if (left <= right && bottom <= top)
                return new Box2i(left, top, right, bottom);

            return new Box2i();
        }

        // override object.Equals
        public override readonly bool Equals(object? obj)
        {
            if (obj is Box2i box)
            {
                return Equals(box);
            }

            return false;
        }

        public readonly bool Equals(Box2i other)
        {
            return other.Left == Left && other.Right == Right && other.Bottom == Bottom && other.Top == Top;
        }

        // override object.GetHashCode
        public override readonly int GetHashCode()
        {
            var code = Left.GetHashCode();
            code = (code * 929) ^ Right.GetHashCode();
            code = (code * 929) ^ Top.GetHashCode();
            code = (code * 929) ^ Bottom.GetHashCode();
            return code;
        }

        public override readonly string ToString()
        {
            return $"({Left}, {Top}, {Right}, {Bottom})";
        }
    }
}
