using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// Randomly selects damage based on dice.
    /// Arguments you can use here:
    /// - "power": Power of the effect
    /// - "skillLevel": Level of the associated skill.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectBaseDamageDiceComponent : Component
    {
        [DataField]
        public PrototypeId<SkillPrototype>? AssociatedSkill { get; }

        [DataField]
        public Formula DiceX { get; } = new("1");

        [DataField]
        public Formula DiceY { get; } = new("1");

        [DataField]
        public Formula Bonus { get; } = new("0");

        [DataField]
        public Formula ElementPower { get; } = new("0");

        [DataField]
        public Formula DamageModifier { get; } = new("baseDamage");
    }

    /// <summary>
    /// Modifies damage based on the caster's Control Magic level.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageControlMagicComponent : Component
    {
    }

    /// <summary>
    /// Controls how the damage message is displayed.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageMessageComponent : Component
    {
        /// <summary>
        /// Root key to use. It should have "Ally" and "Other"
        /// messages for attacks that hit allies and non-allies.
        /// </summary>
        [DataField]
        public LocaleKey RootKey { get; set; }
    }

    /// <summary>
    /// Causes elemental damage to targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageElementalComponent : Component
    {
        [DataField]
        public PrototypeId<ElementPrototype> Element { get; set; } = Prototypes.Protos.Element.Fire;
    }
}
