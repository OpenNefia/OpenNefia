using OpenNefia.Content.PCCs;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Material
{
    [Prototype("Elona.Material")]
    public class MaterialPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
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