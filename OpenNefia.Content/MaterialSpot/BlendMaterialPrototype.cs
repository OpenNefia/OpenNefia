using OpenNefia.Content.PCCs;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.MaterialSpot
{
    [Prototype("Elona.BlendMaterial")]
    public class BlendMaterialPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public int Level { get; } = 0;

        [DataField(required: true)]
        public int Rarity { get; } = 0;

        [DataField(required: true)]
        public PrototypeId<ChipPrototype> Chip { get; }
    }
}