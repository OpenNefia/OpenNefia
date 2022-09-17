using OpenNefia.Content.Effects;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Feats;
using static OpenNefia.Core.Prototypes.EntityPrototype;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.Prototypes
{
    [Prototype("Elona.Race")]
    public class RacePrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public PrototypeId<ChipPrototype> ChipMale { get; } = default!;

        [DataField(required: true)]
        public PrototypeId<ChipPrototype> ChipFemale { get; } = default!;

        [DataField]
        public float MaleRatio { get; } = 0.5f;

        [DataField]
        public int BaseHeight { get; } = 10;

        [DataField]
        public int MinAge { get; } = 1;

        [DataField]
        public int MaxAge { get; } = 100;

        [DataField]
        public int? BreedPower { get; }

        [DataField]
        public bool IsExtra { get; } = false;

        [DataField]
        public IEffect? OnInitPlayer { get; } = null;

        [DataField("baseSkills")]
        private Dictionary<PrototypeId<SkillPrototype>, int> _baseSkills = new();

        public IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> BaseSkills => _baseSkills;

        [DataField("initialEquipSlots", required: true)]
        private List<PrototypeId<EquipSlotPrototype>> _initialEquipSlots = new();

        /// <summary>
        /// Equipment slots to generate this character with.
        /// </summary>
        /// <remarks>
        /// NOTE: This isn't declared on <see cref="InventoryComponent"/> in the entity
        /// prototype since it's indirectly set up during race initialization.
        /// </remarks>
        public IReadOnlyList<PrototypeId<EquipSlotPrototype>> InitialEquipSlots => _initialEquipSlots;

        [DataField("initialFeats")]
        private readonly Dictionary<PrototypeId<FeatPrototype>, int> _initialFeats = new();

        public IReadOnlyDictionary<PrototypeId<FeatPrototype>, int> InitialFeats => _initialFeats;

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }
}
