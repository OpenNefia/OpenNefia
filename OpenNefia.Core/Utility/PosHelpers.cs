using System;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Utility
{
    public static class PosHelpers
    {
        public static double Distance(Vector2i a, Vector2i b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static IEnumerable<Vector2i> EnumerateLine(Vector2i from, Vector2i to, bool includeStartPos = false)
        {
            if (from == to)
            {
                if (includeStartPos)
                    yield return to;
                yield break;
            }

            int deltaX, deltaY, boundX, boundY;

            if (from.X < to.X)
            {
                deltaX = 1;
                boundX = to.X - from.X;
            }
            else
            {
                deltaX = -1;
                boundX = from.X - to.X;
            }
            if (from.Y < to.Y)
            {
                deltaY = 1;
                boundY = to.Y - from.Y;
            }
            else
            {
                deltaY = -1;
                boundY = from.Y - to.Y;
            }

            var i = 0;
            var pos = from;
            var err = boundX - boundY;

            do
            {
                if (i > 0 || includeStartPos)
                    yield return pos;
                i++;

                var e = err * 2;
                if (e > -boundY)
                {
                    err -= boundY;
                    pos.X += deltaX;
                }
                if (e < boundX)
                {
                    err += boundX;
                    pos.Y += deltaY;
                }
            } while (pos != to);

            yield return pos;
        }

        public static IEnumerable<MapCoordinates> EnumerateLine(MapCoordinates a, MapCoordinates b)
        {
            if (a.MapId != b.MapId)
                return Enumerable.Empty<MapCoordinates>();

            return EnumerateLine(a.Position, b.Position).Select(pos => new MapCoordinates(a.MapId, pos));
        }

        public static IEnumerable<MapCoordinates> GetSurroundingCoords(MapCoordinates coords)
        {
            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        yield return coords.Offset(i, j);
                    }
                }
            }
        }

        public static IEnumerable<Vector2i> EnumerateBallPositions(Vector2i origin, int radius, UIBox2i bounds, bool includeStartPos = false)
        {
            for (var i = 0; i <= radius * 2; i++)
            {
                var ty = origin.Y - radius + i;
                if (ty >= bounds.Left && ty < bounds.Right)
                {
                    for (var j = 0; j <= radius * 2; j++)
                    {
                        var tx = origin.X - radius + j;
                        if (tx >= bounds.Left && tx < bounds.Right)
                        {
                            var pos = new Vector2i(tx, ty);
                            // TODO: verify this in HSP.
                            if (double.Floor((pos - origin).Length) <= radius && (includeStartPos || pos != origin))
                            {
                                yield return pos;
                            }
                        }
                    }
                }
            }
        }
    }
}
