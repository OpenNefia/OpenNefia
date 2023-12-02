using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    public static class MapGenUtils
    {
        public static IEnumerable<Vector2i> EnumerateBorder(UIBox2i bounds)
        {
            var start = bounds.TopLeft;
            var end = bounds.BottomRight;
            var pos = start;

            while (pos.Y <= end.Y)
            {
                yield return pos;

                if (pos.Y == start.Y || pos.Y == end.Y - 1)
                {
                    if (pos.X == end.X - 1)
                    {
                        pos.X = start.X;
                        pos.Y++;
                    }
                    else
                    {
                        pos.X++;
                    }
                }
                else
                {
                    if (pos.X == start.X)
                    {
                        pos.X = end.X - 1;
                    }
                    else
                    {
                        pos.X = start.X;
                        pos.Y++;
                    }
                }
            }
        }
        public static IEnumerable<Vector2i> EnumerateBounds(UIBox2i bounds)
        {
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                for (var y = bounds.Top; y < bounds.Bottom; y++)
                {
                    yield return (x, y);
                }
            }
        }
    }
}
