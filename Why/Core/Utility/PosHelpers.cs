using System;
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

        /// <summary>
        /// From http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html.
        /// </summary>
        /// <param name="a.X"></param>
        /// <param name="a.Y"></param>
        /// <param name="b.X"></param>
        /// <param name="b.Y"></param>
        /// <returns></returns>
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
    }
}
