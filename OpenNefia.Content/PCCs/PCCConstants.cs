using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.PCCs
{
    public static class PCCConstants
    {
        public static readonly Color[] DefaultPartColors =
        {
            new (255, 255, 255, 255),
            new (175, 255, 175, 255),
            new (255, 155, 155, 255),
            new (175, 175, 255, 255),
            new (255, 215, 175, 255),
            new (255, 255, 175, 255),
            new (155, 154, 153, 255),
            new (185, 155, 215, 255),
            new (155, 205, 205, 255),
            new (255, 195, 185, 255),
            new (235, 215, 155, 255),
            new (225, 215, 185, 255),
            new (105, 235, 105, 255),
            new (205, 205, 205, 255),
            new (255, 225, 225, 255),
            new (225, 225, 255, 255),
            new (225, 195, 255, 255),
            new (215, 255, 215, 255),
            new (210, 250, 160, 255),
        };

        // TODO less hardcoding
        public static readonly IReadOnlyDictionary<PCCPartType, int> DefaultPartZOrders
            = new Dictionary<PCCPartType, int>()
        {
            { PCCPartType.Mantle, 1000 },
            { PCCPartType.Hairbk, 2000 },
            { PCCPartType.Ridebk, 3000 },
            { PCCPartType.Body, 4000 },
            { PCCPartType.Eye, 5000 },
            { PCCPartType.Pants, 6000 },
            { PCCPartType.Cloth, 7000 },
            { PCCPartType.Chest, 8000 },
            { PCCPartType.Leg, 9000 },
            { PCCPartType.Belt, 10000 },
            { PCCPartType.Glove, 11000 },
            { PCCPartType.Ride, 12000 },
            { PCCPartType.Mantlebk, 13000 },
            { PCCPartType.Hair, 14000 },
            { PCCPartType.Subhair, 15000 },
            { PCCPartType.Etc, 16000 },
            { PCCPartType.Boots, 17000 }
        };

        /// <summary>
        /// The set of PCC parts that should be set by default.
        /// </summary>
        /// <remarks>
        /// This is necessary since characters can set more than one <see cref="PCCPartType.Etc"/> 
        /// slot in the appearance settings menu.
        /// </remarks>
        public static readonly IReadOnlyDictionary<string, PCCPartType> DefaultPartLayout
            = new Dictionary<string, PCCPartType>()
        {
            { PCCPartSlots.Mantle,   PCCPartType.Mantle },
            { PCCPartSlots.Hairbk,   PCCPartType.Hairbk },
            { PCCPartSlots.Ridebk,   PCCPartType.Ridebk },
            { PCCPartSlots.Body,     PCCPartType.Body },
            { PCCPartSlots.Eye,      PCCPartType.Eye },
            { PCCPartSlots.Pants,    PCCPartType.Pants },
            { PCCPartSlots.Cloth,    PCCPartType.Cloth },
            { PCCPartSlots.Chest,    PCCPartType.Chest },
            { PCCPartSlots.Leg,      PCCPartType.Leg },
            { PCCPartSlots.Belt,     PCCPartType.Belt },
            { PCCPartSlots.Glove,    PCCPartType.Glove },
            { PCCPartSlots.Ride,     PCCPartType.Ride },
            { PCCPartSlots.Mantlebk, PCCPartType.Mantlebk },
            { PCCPartSlots.Hair,     PCCPartType.Hair },
            { PCCPartSlots.SubHair,  PCCPartType.Subhair },
            { PCCPartSlots.Etc1,     PCCPartType.Etc },
            { PCCPartSlots.Etc2,     PCCPartType.Etc },
            { PCCPartSlots.Etc3,     PCCPartType.Etc },
            { PCCPartSlots.Boots,    PCCPartType.Boots }
        };
    }
}
