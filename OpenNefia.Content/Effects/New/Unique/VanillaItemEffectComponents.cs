using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects.New.Unique
{
    /// <summary>
    /// Spawns a new character and recruits them as an ally.
    /// </summary>
    [RegisterComponent]
    public sealed class EffectGainAllyComponent : Component
    {
        [DataField]
        public CharaFilter CharaFilter { get; set; } = new();

        [DataField]
        public LocaleKey? MessageKey { get; set; }
    }

    [RegisterComponent]
    public sealed class EffectChangeMaterialComponent : Component
    {
        /// <summary>
        /// <c>true</c> for the material kit item.
        /// </summary>
        [DataField]
        public bool ApplyToGodlyAndLivingWeapons { get; set; } = false;
    }

    [RegisterComponent]
    public sealed class EffectGaroksHammerComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectMilkComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectAleComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectWaterComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectGainKnowledgeComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectGainSkillComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectPunishDecrementStatsComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectGainFaithComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectPoisonComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectConfuseComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectParalyzeComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectBlindComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectSleepComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectGainPotentialComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectCurseComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectDeedComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectSulfuricComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectCreateMaterialComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectWeakenResistanceComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectGainSkillPotentialComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectElixirComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectCureMutationComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectEnchantWeaponComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectEnchantArmorComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectDeedOfInheritanceComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectRechargeComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectDirtyWaterComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectCureCorruptionComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectAlchemyComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectMolotovComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectLovePotionComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectTreasureMapComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectTrollBloodComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectFlightComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectEscapeComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectSaltComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectDescentComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectEvolutionComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectNameComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectSodaComponent : Component
    {
    }

    [RegisterComponent]
    public sealed class EffectCupsuleComponent : Component
    {
    }
}