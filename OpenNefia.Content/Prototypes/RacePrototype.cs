using OpenNefia.Content.Effects;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Prototypes
{
    [Prototype("Race")]
    public class RacePrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
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

        [DataField(required: true)]
        public Dictionary<PrototypeId<SkillPrototype>, int> BaseSkills = new();
    }
}
