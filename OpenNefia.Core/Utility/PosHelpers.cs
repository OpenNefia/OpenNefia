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

        /// <remarks>
        /// From http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html.
        /// </remarks>
        public static IEnumerable<Vector2i> EnumerateLine(Vector2i a, Vector2i b)
        {
            bool steep = Math.Abs(b.Y - a.Y) > Math.Abs(b.X - a.X);
            if (steep)
            {
                int t;
                t = a.X; // swap a.X and a.Y
                a.X = a.Y;
                a.Y = t;
                t = b.X; // swap b.X and b.Y
                b.X = b.Y;
                b.Y = t;
            }
            if (a.X > b.X)
            {
                int t;
                t = a.X; // swap a.X and b.X
                a.X = b.X;
                b.X = t;
                t = a.Y; // swap a.Y and b.Y
                a.Y = b.Y;
                b.Y = t;
            }
            int dx = b.X - a.X;
            int dy = Math.Abs(b.Y - a.Y);
            int error = dx / 2;
            int ystep = (a.Y < b.Y) ? 1 : -1;
            int y = a.Y;
            for (int x = a.X; x <= b.X; x++)
            {
                yield return new Vector2i((steep ? y : x), (steep ? x : y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }

        public static IEnumerable<MapCoordinates> EnumerateLine(MapCoordinates a, MapCoordinates b)
        {
            if (a.MapId != b.MapId)
                return Enumerable.Empty<MapCoordinates>();

            return EnumerateLine(a.Position, b.Position).Select(pos => new MapCoordinates(pos, a.MapId));
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
    }
}
