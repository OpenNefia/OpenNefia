using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Effects;
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
        public bool IsExtra { get; } = false;

        [DataField]
        public IEffect? OnInitPlayer { get; } = null;

        [DataField(required: true)]
        public Dictionary<PrototypeId<SkillPrototype>, int> BaseSkills = new();
    }
}
