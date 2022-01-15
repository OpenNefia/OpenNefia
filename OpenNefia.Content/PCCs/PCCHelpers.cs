using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static PCCDrawable MakeDefaultPCC(IPrototypeManager protos, IResourceCache resourceCache)
        {
            var allParts = protos
                .EnumeratePrototypes<PCCPartPrototype>()
                .GroupBy(part => part.PCCPartType)
                .Select(group => group.FirstOrDefault())
                .WhereNotNull()
                .Select(part => new PCCPart(part.PCCPartType, part.ImagePath, Color.White));

            var pccDrawable = new PCCDrawable(allParts);
            pccDrawable.RebakeImage(resourceCache);
            return pccDrawable;
        }
    }
}
