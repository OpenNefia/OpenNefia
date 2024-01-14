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
