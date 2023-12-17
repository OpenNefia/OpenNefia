using SpellPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Spells.SpellPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Spell
        {
            #pragma warning disable format

            #region Spells

            public static readonly SpellPrototypeId Gravity               = new($"Elona.{nameof(Gravity)}");
            public static readonly SpellPrototypeId Dominate              = new($"Elona.{nameof(Dominate)}");
            public static readonly SpellPrototypeId Return                = new($"Elona.{nameof(Return)}");
            public static readonly SpellPrototypeId FourDimensionalPocket = new($"Elona.{nameof(FourDimensionalPocket)}");
            public static readonly SpellPrototypeId MagicMap              = new($"Elona.{nameof(MagicMap)}");
            public static readonly SpellPrototypeId SenseObject           = new($"Elona.{nameof(SenseObject)}");
            public static readonly SpellPrototypeId Identify              = new($"Elona.{nameof(Identify)}");
            public static readonly SpellPrototypeId Uncurse               = new($"Elona.{nameof(Uncurse)}");
            public static readonly SpellPrototypeId Oracle                = new($"Elona.{nameof(Oracle)}");
            public static readonly SpellPrototypeId WallCreation          = new($"Elona.{nameof(WallCreation)}");
            public static readonly SpellPrototypeId DoorCreation          = new($"Elona.{nameof(DoorCreation)}");
            public static readonly SpellPrototypeId WizardsHarvest        = new($"Elona.{nameof(WizardsHarvest)}");
            public static readonly SpellPrototypeId RestoreBody           = new($"Elona.{nameof(RestoreBody)}");
            public static readonly SpellPrototypeId RestoreSpirit         = new($"Elona.{nameof(RestoreSpirit)}");
            public static readonly SpellPrototypeId Wish                  = new($"Elona.{nameof(Wish)}");
            public static readonly SpellPrototypeId Resurrection          = new($"Elona.{nameof(Resurrection)}");
            public static readonly SpellPrototypeId Meteor                = new($"Elona.{nameof(Meteor)}");
            public static readonly SpellPrototypeId IceBolt               = new($"Elona.{nameof(IceBolt)}");
            public static readonly SpellPrototypeId FireBolt              = new($"Elona.{nameof(FireBolt)}");
            public static readonly SpellPrototypeId LightningBolt         = new($"Elona.{nameof(LightningBolt)}");
            public static readonly SpellPrototypeId DarknessBolt          = new($"Elona.{nameof(DarknessBolt)}");
            public static readonly SpellPrototypeId MindBolt              = new($"Elona.{nameof(MindBolt)}");
            public static readonly SpellPrototypeId MagicDart             = new($"Elona.{nameof(MagicDart)}");
            public static readonly SpellPrototypeId NetherArrow           = new($"Elona.{nameof(NetherArrow)}");
            public static readonly SpellPrototypeId NerveArrow            = new($"Elona.{nameof(NerveArrow)}");
            public static readonly SpellPrototypeId ChaosEye              = new($"Elona.{nameof(ChaosEye)}");
            public static readonly SpellPrototypeId DarkEye               = new($"Elona.{nameof(DarkEye)}");
            public static readonly SpellPrototypeId CrystalSpear          = new($"Elona.{nameof(CrystalSpear)}");
            public static readonly SpellPrototypeId IceBall               = new($"Elona.{nameof(IceBall)}");
            public static readonly SpellPrototypeId FireBall              = new($"Elona.{nameof(FireBall)}");
            public static readonly SpellPrototypeId ChaosBall             = new($"Elona.{nameof(ChaosBall)}");
            public static readonly SpellPrototypeId RagingRoar            = new($"Elona.{nameof(RagingRoar)}");
            public static readonly SpellPrototypeId MagicStorm            = new($"Elona.{nameof(MagicStorm)}");
            public static readonly SpellPrototypeId HealingRain           = new($"Elona.{nameof(HealingRain)}");
            public static readonly SpellPrototypeId HealLight             = new($"Elona.{nameof(HealLight)}");
            public static readonly SpellPrototypeId HealCritical          = new($"Elona.{nameof(HealCritical)}");
            public static readonly SpellPrototypeId HealingTouch          = new($"Elona.{nameof(HealingTouch)}");
            public static readonly SpellPrototypeId CureOfEris            = new($"Elona.{nameof(CureOfEris)}");
            public static readonly SpellPrototypeId CureOfJure            = new($"Elona.{nameof(CureOfJure)}");
            public static readonly SpellPrototypeId Teleport              = new($"Elona.{nameof(Teleport)}");
            public static readonly SpellPrototypeId TeleportOther         = new($"Elona.{nameof(TeleportOther)}");
            public static readonly SpellPrototypeId ShortTeleport         = new($"Elona.{nameof(ShortTeleport)}");
            public static readonly SpellPrototypeId HolyLight             = new($"Elona.{nameof(HolyLight)}");
            public static readonly SpellPrototypeId VanquishHex           = new($"Elona.{nameof(VanquishHex)}");
            public static readonly SpellPrototypeId SummonMonsters        = new($"Elona.{nameof(SummonMonsters)}");
            public static readonly SpellPrototypeId SummonWild            = new($"Elona.{nameof(SummonWild)}");
            public static readonly SpellPrototypeId Mutation              = new($"Elona.{nameof(Mutation)}");
            public static readonly SpellPrototypeId Web                   = new($"Elona.{nameof(Web)}");
            public static readonly SpellPrototypeId MistOfDarkness        = new($"Elona.{nameof(MistOfDarkness)}");
            public static readonly SpellPrototypeId AcidGround            = new($"Elona.{nameof(AcidGround)}");
            public static readonly SpellPrototypeId FireWall              = new($"Elona.{nameof(FireWall)}");

            #endregion

            #region Buffs

            public static readonly SpellPrototypeId BuffHolyShield      = new($"Elona.{nameof(BuffHolyShield)}");
            public static readonly SpellPrototypeId BuffMistOfSilence   = new($"Elona.{nameof(BuffMistOfSilence)}");
            public static readonly SpellPrototypeId BuffRegeneration    = new($"Elona.{nameof(BuffRegeneration)}");
            public static readonly SpellPrototypeId BuffElementalShield = new($"Elona.{nameof(BuffElementalShield)}");
            public static readonly SpellPrototypeId BuffSpeed           = new($"Elona.{nameof(BuffSpeed)}");
            public static readonly SpellPrototypeId BuffSlow            = new($"Elona.{nameof(BuffSlow)}");
            public static readonly SpellPrototypeId BuffHero            = new($"Elona.{nameof(BuffHero)}");
            public static readonly SpellPrototypeId BuffMistOfFrailness = new($"Elona.{nameof(BuffMistOfFrailness)}");
            public static readonly SpellPrototypeId BuffElementScar     = new($"Elona.{nameof(BuffElementScar)}");
            public static readonly SpellPrototypeId BuffHolyVeil        = new($"Elona.{nameof(BuffHolyVeil)}");
            public static readonly SpellPrototypeId BuffNightmare       = new($"Elona.{nameof(BuffNightmare)}");
            public static readonly SpellPrototypeId BuffDivineWisdom    = new($"Elona.{nameof(BuffDivineWisdom)}");
            public static readonly SpellPrototypeId BuffPunishment      = new($"Elona.{nameof(BuffPunishment)}");
            public static readonly SpellPrototypeId BuffLulwysTrick     = new($"Elona.{nameof(BuffLulwysTrick)}");
            public static readonly SpellPrototypeId BuffIncognito       = new($"Elona.{nameof(BuffIncognito)}");
            public static readonly SpellPrototypeId BuffDeathWord       = new($"Elona.{nameof(BuffDeathWord)}");
            public static readonly SpellPrototypeId BuffBoost           = new($"Elona.{nameof(BuffBoost)}");
            public static readonly SpellPrototypeId BuffContingency     = new($"Elona.{nameof(BuffContingency)}");

            #endregion

            #region Effects

            //public static readonly SpellPrototypeId EffectMaterialKit =          new($"Elona.{nameof(EffectMaterialKit)}");
            //public static readonly SpellPrototypeId EffectGaroksHammer =         new($"Elona.{nameof(EffectGaroksHammer)}");
            //public static readonly SpellPrototypeId EffectMilk =                 new($"Elona.{nameof(EffectMilk)}");
            //public static readonly SpellPrototypeId EffectAle =                  new($"Elona.{nameof(EffectAle)}");
            //public static readonly SpellPrototypeId EffectWater =                new($"Elona.{nameof(EffectWater)}");
            //public static readonly SpellPrototypeId EffectGainKnowledge =        new($"Elona.{nameof(EffectGainKnowledge)}");
            //public static readonly SpellPrototypeId EffectGainSkill =            new($"Elona.{nameof(EffectGainSkill)}");
            //public static readonly SpellPrototypeId EffectPunishDecrementStats = new($"Elona.{nameof(EffectPunishDecrementStats)}");
            //public static readonly SpellPrototypeId EffectGainFaith =            new($"Elona.{nameof(EffectGainFaith)}");
            //public static readonly SpellPrototypeId EffectPoison =               new($"Elona.{nameof(EffectPoison)}");
            //public static readonly SpellPrototypeId EffectConfuse =              new($"Elona.{nameof(EffectConfuse)}");
            //public static readonly SpellPrototypeId EffectParalyze =             new($"Elona.{nameof(EffectParalyze)}");
            //public static readonly SpellPrototypeId EffectBlind =                new($"Elona.{nameof(EffectBlind)}");
            //public static readonly SpellPrototypeId EffectSleep =                new($"Elona.{nameof(EffectSleep)}");
            //public static readonly SpellPrototypeId EffectGainPotential =        new($"Elona.{nameof(EffectGainPotential)}");
            //public static readonly SpellPrototypeId EffectCurse =                new($"Elona.{nameof(EffectCurse)}");
            //public static readonly SpellPrototypeId EffectDeed =                 new($"Elona.{nameof(EffectDeed)}");
            //public static readonly SpellPrototypeId EffectSulfuric =             new($"Elona.{nameof(EffectSulfuric)}");
            //public static readonly SpellPrototypeId EffectCreateMaterial =       new($"Elona.{nameof(EffectCreateMaterial)}");
            //public static readonly SpellPrototypeId EffectWeakenResistance =     new($"Elona.{nameof(EffectWeakenResistance)}");
            //public static readonly SpellPrototypeId EffectGainSkillPotential =   new($"Elona.{nameof(EffectGainSkillPotential)}");
            //public static readonly SpellPrototypeId EffectElixir =               new($"Elona.{nameof(EffectElixir)}");
            //public static readonly SpellPrototypeId EffectCureMutation =         new($"Elona.{nameof(EffectCureMutation)}");
            //public static readonly SpellPrototypeId EffectGainAlly =             new($"Elona.{nameof(EffectGainAlly)}");
            //public static readonly SpellPrototypeId EffectGainYoungerSister =    new($"Elona.{nameof(EffectGainYoungerSister)}");
            //public static readonly SpellPrototypeId EffectEnchantWeapon =        new($"Elona.{nameof(EffectEnchantWeapon)}");
            //public static readonly SpellPrototypeId EffectEnchantArmor =         new($"Elona.{nameof(EffectEnchantArmor)}");
            //public static readonly SpellPrototypeId EffectChangeMaterial =       new($"Elona.{nameof(EffectChangeMaterial)}");
            //public static readonly SpellPrototypeId EffectDeedOfInheritance =    new($"Elona.{nameof(EffectDeedOfInheritance)}");
            //public static readonly SpellPrototypeId EffectRecharge =             new($"Elona.{nameof(EffectRecharge)}");
            //public static readonly SpellPrototypeId EffectDirtyWater =           new($"Elona.{nameof(EffectDirtyWater)}");
            //public static readonly SpellPrototypeId EffectCureCorruption =       new($"Elona.{nameof(EffectCureCorruption)}");
            //public static readonly SpellPrototypeId EffectAlchemy =              new($"Elona.{nameof(EffectAlchemy)}");
            //public static readonly SpellPrototypeId EffectMolotov =              new($"Elona.{nameof(EffectMolotov)}");
            //public static readonly SpellPrototypeId EffectLovePotion =           new($"Elona.{nameof(EffectLovePotion)}");
            //public static readonly SpellPrototypeId EffectTreasureMap =          new($"Elona.{nameof(EffectTreasureMap)}");
            //public static readonly SpellPrototypeId EffectGainYoungLady =        new($"Elona.{nameof(EffectGainYoungLady)}");
            //public static readonly SpellPrototypeId EffectGainCatSister =        new($"Elona.{nameof(EffectGainCatSister)}");
            //public static readonly SpellPrototypeId EffectTrollBlood =           new($"Elona.{nameof(EffectTrollBlood)}");
            //public static readonly SpellPrototypeId EffectFlight =               new($"Elona.{nameof(EffectFlight)}");
            //public static readonly SpellPrototypeId EffectEscape =               new($"Elona.{nameof(EffectEscape)}");
            //public static readonly SpellPrototypeId EffectSalt =                 new($"Elona.{nameof(EffectSalt)}");
            //public static readonly SpellPrototypeId EffectDescent =              new($"Elona.{nameof(EffectDescent)}");
            //public static readonly SpellPrototypeId EffectEvolution =            new($"Elona.{nameof(EffectEvolution)}");
            //public static readonly SpellPrototypeId EffectName =                 new($"Elona.{nameof(EffectName)}");
            //public static readonly SpellPrototypeId EffectSoda =                 new($"Elona.{nameof(EffectSoda)}");
            //public static readonly SpellPrototypeId EffectCupsule =              new($"Elona.{nameof(EffectCupsule)}");

            #endregion

            #pragma warning restore format
        }
    }
}
