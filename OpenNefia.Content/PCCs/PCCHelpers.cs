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

        /// <summary>
        /// Gets a dictionary of all the PCC parts grouped by their <see cref="PCCPartType"/>.
        /// </summary>
        public static Dictionary<PCCPartType, List<MapTilesetPrototype>> GetGroupedPCCPartPrototypes(IPrototypeManager protos)
        {
            return protos
                .EnumeratePrototypes<MapTilesetPrototype>()
                .GroupBy(part => part.PCCPartType)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        /// <summary>
        /// Instantiates all PCC part prototypes as <see cref="PCCPart"/> instances, grouped by PCC part type.
        /// </summary>
        public static Dictionary<PCCPartType, List<PCCPart>> GetGroupedPCCParts(IPrototypeManager protos)
        {
            return GetGroupedPCCPartPrototypes(protos)
                .ToDictionary(pair => pair.Key,
                              pair => pair.Value.Select(MakePCCPartFromPrototype).ToList());
        }

        public static int GetPCCPartTypeZOrder(PCCPartType type)
        {
            return PCCConstants.DefaultPartZOrders.GetValueOr(type, PCCConstants.DefaultPCCPartZOrder);
        }

        /// <summary>
        /// Makes a <see cref="PCCPart"/> from a <see cref="MapTilesetPrototype"/>.
        /// </summary>
        private static PCCPart MakePCCPartFromPrototype(MapTilesetPrototype proto)
        {
            var zOrder = GetPCCPartTypeZOrder(proto.PCCPartType);
            return new PCCPart(proto.PCCPartType, proto.ImagePath, Color.White, zOrder);
        }

        /// <summary>
        /// Initializes a new PCC drawable from a given mapping of PCC slot ID names to the types of PCC parts
        /// those slots should start with.
        /// </summary>
        /// <param name="partLayout">Map of PCC slot name -> PCC part type.</param>
        public static PCCDrawable CreateDefaultPCCFromLayout(IReadOnlyDictionary<string, PCCPartType> partLayout, IPrototypeManager protos, IResourceCache resourceCache)
        {
            // Grab the first PCC part by prototype order.
            // (Unlike with chips/portraits, there is no "default" PCC part prototype, so First() will
            // always return an actual PCC part here.)
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
