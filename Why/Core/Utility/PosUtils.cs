using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Utility
{
    public static class PosUtils
    {
        public static IEnumerable<MapCoordinates> GetSurroundingCoords(MapCoordinates coords)
        {
            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        yield return new MapCoordinates(coords.MapId, coords.Position + new Vector2i(i, j));
                    }
                }
            }
        }
    }
}
