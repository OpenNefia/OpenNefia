using OpenNefia.Core.Maths;
using OpenNefia.Content.CharaAppearance;

namespace OpenNefia.Content.PCCs
{
    public static class PCCConstants
    {
        /// <summary>
        /// The default set of PCC part colors selectable in the <see cref="CharaAppearanceControl"/>.
        /// </summary>
        public static readonly Color[] DefaultPCCPartColors =
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

        /// <summary>
        /// Fallback Z-order for unknown PCC part types.
        /// </summary>
        public const int DefaultPCCPartZOrder = 100000;

        /// <summary>
        /// Default Z-ordering for all PCC part types.
        /// </summary>
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
        public static readonly IReadOnlyDictionary<string, PCCPartType> DefaultPCCPartLayout
            = new Dictionary<string, PCCPartType>()
        {
            { PCCPartSlots.Body,     PCCPartType.Body },
            { PCCPartSlots.Eye,      PCCPartType.Eye },
            { PCCPartSlots.Pants,    PCCPartType.Pants },
            { PCCPartSlots.Cloth,    PCCPartType.Cloth },
            { PCCPartSlots.Hair,     PCCPartType.Hair },
            { PCCPartSlots.SubHair,  PCCPartType.Subhair },
        };
    }
}
