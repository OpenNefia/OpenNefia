using OpenNefia.Content.Skills;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Buffs
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffHolyShieldComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffMistOfSilenceComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffRegenerationComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffElementalShieldComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffSpeedComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffSlowComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffHeroComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffMistOfFrailnessComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffElementScarComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffHolyVeilComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffNightmareComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffDivineWisdomComponent : Component
    {
        [DataField]
        public Formula LearningMagic { get; set; } = new Formula("power");

        [DataField]
        public Formula Literacy { get; set; } = new Formula("power");
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffPunishmentComponent : Component
    {
        [DataField]
        public double PVModifier { get; set; } = 0.8;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffLulwysTrickComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffIncognitoComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffDeathWordComponent : Component
    {
    }

    /// <summary>
    /// Keeps track of entities this entity has applied Death Word to.
    /// If the entity is killed, all targets will have the death word revoked.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class DeathWordTargetsComponent : Component
    {
        [DataField]
        public HashSet<EntityUid> Targets { get; set; } = new();
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffBoostComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffContingencyComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffLuckyComponent : Component
    {
    }

    /// <summary>
    /// Shared between all food buffs.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffFoodComponent : Component
    {
        /// <summary>
        /// Skill to affect. Usually an attribute.
        /// </summary>
        [DataField]
        public PrototypeId<SkillPrototype> Skill { get; set; }
    }
}
