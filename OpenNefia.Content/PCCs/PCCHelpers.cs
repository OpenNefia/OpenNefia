using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.PCCs.PCCDrawable;

namespace OpenNefia.Content.PCCs
{
    public static class PCCHelpers
    {
        public static PCCDirection ToPCCDirection(this Direction dir)
        {
            switch (dir)
            {
                case Direction.West:
                    return PCCDirection.West;
                case Direction.East:
                    return PCCDirection.East;
                case Direction.North:
                case Direction.NorthWest:
                case Direction.NorthEast:
                    return PCCDirection.North;
                case Direction.South:
                case Direction.SouthWest:
                case Direction.SouthEast:
                default:
                    return PCCDirection.South;
            }
        }
    }
}
