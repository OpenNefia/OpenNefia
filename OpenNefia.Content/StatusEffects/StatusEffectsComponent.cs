using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.StatusEffects
{
    [RegisterComponent]
    public sealed class StatusEffectsComponent : Component
    {
        public override string Name => "StatusEffects";

        [DataField]
        public Dictionary<PrototypeId<StatusEffectPrototype>, StatusEffect> StatusEffects { get; } = new();

        [DataField]
        public HashSet<PrototypeId<StatusEffectPrototype>> StatusEffectImmunities { get; } = new();
    }

    [DataDefinition]
    public sealed class StatusEffect
    {
        [DataField(required: true)]
        public int TurnsRemaining { get; set; } = 0;

        [DataField(required: true)]
        public SlotId Slot { get; set; }
    }
}
