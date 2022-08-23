using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Enchantments
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncRandomTeleportComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncRandomTeleport";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSuckBloodComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSuckBlood";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSuckExperienceComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSuckExperience";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSummonCreatureComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSummonCreature";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncPreventTeleportComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncPreventTeleport";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistBlindnessComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistBlindness";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistParalysisComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistParalysis";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistConfusionComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistConfusion";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistFearComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistFear";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistSleepComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistSleep";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistPoisonComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistPoison";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistTheftComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistTheft";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistRottenFoodComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistRottenFood";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncFastTravelComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncFastTravel";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistEtherwindComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistEtherwind";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistBadWeatherComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistBadWeather";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistPregnancyComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistPregnancy";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncFloatComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncFloat";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistMutationComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistMutation";
    }


    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncEnhanceSpellsComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncEnhanceSpells";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSeeInvisibleComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSeeInvisible";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncAbsorbStaminaComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncAbsorbStamina";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncRagnarokComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncRagnarok";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncAbsorbManaComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncAbsorbMana";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncPierceChanceComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncPierceChance";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncCriticalChanceComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncCriticalChance";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncExtraMeleeAttackChanceComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncExtraMeleeAttackChance";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncExtraRangedAttackChanceComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncExtraRangedAttackChance";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncTimeStopComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncTimeStop";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistCurseComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistCurse";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncStradivariusComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncStradivarius";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncDamageResistanceComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncDamageResistance";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncDamageImmunityComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncDamageImmunity";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncDamageReflectionComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncDamageReflection";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncCuresBleedingQuicklyComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncCuresBleedingQuickly";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncCatchesGodSignalsComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncCatchesGodSignals";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncDragonBaneComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncDragonBane";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncUndeadBaneComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncUndeadBane";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncDetectReligionComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncDetectReligion";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncGouldComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncGould";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncGodBaneComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncGodBane";
    }
}
