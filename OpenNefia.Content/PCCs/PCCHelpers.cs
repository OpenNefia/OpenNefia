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

        public static Dictionary<PCCPartType, List<PCCPartPrototype>> GetGroupedPCCPartPrototypes(IPrototypeManager protos)
        {
            return protos
                .EnumeratePrototypes<PCCPartPrototype>()
                .GroupBy(part => part.PCCPartType)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        public static Dictionary<PCCPartType, List<PCCPart>> GetGroupedPCCParts(IPrototypeManager protos)
        {
            return GetGroupedPCCPartPrototypes(protos)
                .ToDictionary(pair => pair.Key,
                              pair => pair.Value.Select(MakePCCPartFromPrototype).ToList());
        }

        private static PCCPart MakePCCPartFromPrototype(PCCPartPrototype proto)
        {
            return new PCCPart(proto.PCCPartType, proto.ImagePath, Color.White);
        }

        public static PCCDrawable CreateDefaultPCCFromLayout(IReadOnlyDictionary<string, PCCPartType> partLayout, IPrototypeManager protos, IResourceCache resourceCache)
        {
            var defaultParts = GetGroupedPCCParts(protos)
                .ToDictionary(pair => pair.Key, pair => pair.Value.First());

            var parts = partLayout
                .ToDictionary(pair => pair.Key, pair => defaultParts[pair.Value]);

            var pccDrawable = new PCCDrawable(parts);
            pccDrawable.RebakeImage(resourceCache);
            return pccDrawable;
        }
    }
}
