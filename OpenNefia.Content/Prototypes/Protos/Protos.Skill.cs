using SkillPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Skills.SkillPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Skill
        {
            #pragma warning disable format

            #region Stats

            public static readonly SkillPrototypeId AttrLife         = new($"Elona.{nameof(AttrLife)}");
            public static readonly SkillPrototypeId AttrMana         = new($"Elona.{nameof(AttrMana)}");
            public static readonly SkillPrototypeId AttrStrength     = new($"Elona.{nameof(AttrStrength)}");
            public static readonly SkillPrototypeId AttrConstitution = new($"Elona.{nameof(AttrConstitution)}");
            public static readonly SkillPrototypeId AttrDexterity    = new($"Elona.{nameof(AttrDexterity)}");
            public static readonly SkillPrototypeId AttrPerception   = new($"Elona.{nameof(AttrPerception)}");
            public static readonly SkillPrototypeId AttrLearning     = new($"Elona.{nameof(AttrLearning)}");
            public static readonly SkillPrototypeId AttrWill         = new($"Elona.{nameof(AttrWill)}");
            public static readonly SkillPrototypeId AttrMagic        = new($"Elona.{nameof(AttrMagic)}");
            public static readonly SkillPrototypeId AttrCharisma     = new($"Elona.{nameof(AttrCharisma)}");
            public static readonly SkillPrototypeId AttrSpeed        = new($"Elona.{nameof(AttrSpeed)}");
            public static readonly SkillPrototypeId AttrLuck         = new($"Elona.{nameof(AttrLuck)}");

            #endregion

            #region Skills

            public static readonly SkillPrototypeId LongSword      = new($"Elona.{nameof(LongSword)}");
            public static readonly SkillPrototypeId ShortSword     = new($"Elona.{nameof(ShortSword)}");
            public static readonly SkillPrototypeId Axe            = new($"Elona.{nameof(Axe)}");
            public static readonly SkillPrototypeId Blunt          = new($"Elona.{nameof(Blunt)}");
            public static readonly SkillPrototypeId Polearm        = new($"Elona.{nameof(Polearm)}");
            public static readonly SkillPrototypeId Stave          = new($"Elona.{nameof(Stave)}");
            public static readonly SkillPrototypeId MartialArts    = new($"Elona.{nameof(MartialArts)}");
            public static readonly SkillPrototypeId Scythe         = new($"Elona.{nameof(Scythe)}");
            public static readonly SkillPrototypeId Bow            = new($"Elona.{nameof(Bow)}");
            public static readonly SkillPrototypeId Crossbow       = new($"Elona.{nameof(Crossbow)}");
            public static readonly SkillPrototypeId Firearm        = new($"Elona.{nameof(Firearm)}");
            public static readonly SkillPrototypeId Throwing       = new($"Elona.{nameof(Throwing)}");
            public static readonly SkillPrototypeId Shield         = new($"Elona.{nameof(Shield)}");
            public static readonly SkillPrototypeId Evasion        = new($"Elona.{nameof(Evasion)}");
            public static readonly SkillPrototypeId DualWield      = new($"Elona.{nameof(DualWield)}");
            public static readonly SkillPrototypeId TwoHand        = new($"Elona.{nameof(TwoHand)}");
            public static readonly SkillPrototypeId WeightLifting  = new($"Elona.{nameof(WeightLifting)}");
            public static readonly SkillPrototypeId Tactics        = new($"Elona.{nameof(Tactics)}");
            public static readonly SkillPrototypeId Marksman       = new($"Elona.{nameof(Marksman)}");
            public static readonly SkillPrototypeId Healing        = new($"Elona.{nameof(Healing)}");
            public static readonly SkillPrototypeId Mining         = new($"Elona.{nameof(Mining)}");
            public static readonly SkillPrototypeId Carpentry      = new($"Elona.{nameof(Carpentry)}");
            public static readonly SkillPrototypeId HeavyArmor     = new($"Elona.{nameof(HeavyArmor)}");
            public static readonly SkillPrototypeId MediumArmor    = new($"Elona.{nameof(MediumArmor)}");
            public static readonly SkillPrototypeId LightArmor     = new($"Elona.{nameof(LightArmor)}");
            public static readonly SkillPrototypeId LockPicking    = new($"Elona.{nameof(LockPicking)}");
            public static readonly SkillPrototypeId DisarmTrap     = new($"Elona.{nameof(DisarmTrap)}");
            public static readonly SkillPrototypeId Tailoring      = new($"Elona.{nameof(Tailoring)}");
            public static readonly SkillPrototypeId Jeweler        = new($"Elona.{nameof(Jeweler)}");
            public static readonly SkillPrototypeId Stealth        = new($"Elona.{nameof(Stealth)}");
            public static readonly SkillPrototypeId Detection      = new($"Elona.{nameof(Detection)}");
            public static readonly SkillPrototypeId SenseQuality   = new($"Elona.{nameof(SenseQuality)}");
            public static readonly SkillPrototypeId EyeOfMind      = new($"Elona.{nameof(EyeOfMind)}");
            public static readonly SkillPrototypeId GreaterEvasion = new($"Elona.{nameof(GreaterEvasion)}");
            public static readonly SkillPrototypeId Anatomy        = new($"Elona.{nameof(Anatomy)}");
            public static readonly SkillPrototypeId Literacy       = new($"Elona.{nameof(Literacy)}");
            public static readonly SkillPrototypeId Memorization   = new($"Elona.{nameof(Memorization)}");
            public static readonly SkillPrototypeId Alchemy        = new($"Elona.{nameof(Alchemy)}");
            public static readonly SkillPrototypeId Gardening      = new($"Elona.{nameof(Gardening)}");
            public static readonly SkillPrototypeId GeneEngineer   = new($"Elona.{nameof(GeneEngineer)}");
            public static readonly SkillPrototypeId Meditation     = new($"Elona.{nameof(Meditation)}");
            public static readonly SkillPrototypeId MagicDevice    = new($"Elona.{nameof(MagicDevice)}");
            public static readonly SkillPrototypeId Casting        = new($"Elona.{nameof(Casting)}");
            public static readonly SkillPrototypeId ControlMagic   = new($"Elona.{nameof(ControlMagic)}");
            public static readonly SkillPrototypeId MagicCapacity  = new($"Elona.{nameof(MagicCapacity)}");
            public static readonly SkillPrototypeId Faith          = new($"Elona.{nameof(Faith)}");
            public static readonly SkillPrototypeId Traveling      = new($"Elona.{nameof(Traveling)}");
            public static readonly SkillPrototypeId Negotiation    = new($"Elona.{nameof(Negotiation)}");
            public static readonly SkillPrototypeId Investing      = new($"Elona.{nameof(Investing)}");
            public static readonly SkillPrototypeId Performer      = new($"Elona.{nameof(Performer)}");
            public static readonly SkillPrototypeId Cooking        = new($"Elona.{nameof(Cooking)}");
            public static readonly SkillPrototypeId Fishing        = new($"Elona.{nameof(Fishing)}");
            public static readonly SkillPrototypeId Pickpocket     = new($"Elona.{nameof(Pickpocket)}");
            public static readonly SkillPrototypeId Riding         = new($"Elona.{nameof(Riding)}");

            #endregion

            #region Spells

            public static readonly SkillPrototypeId SpellHealLight             = new($"Elona.{nameof(SpellHealLight)}");
            public static readonly SkillPrototypeId SpellHealCritical          = new($"Elona.{nameof(SpellHealCritical)}");
            public static readonly SkillPrototypeId SpellCureOfEris            = new($"Elona.{nameof(SpellCureOfEris)}");
            public static readonly SkillPrototypeId SpellCureOfJure            = new($"Elona.{nameof(SpellCureOfJure)}");
            public static readonly SkillPrototypeId SpellHealingRain           = new($"Elona.{nameof(SpellHealingRain)}");
            public static readonly SkillPrototypeId SpellHealingTouch          = new($"Elona.{nameof(SpellHealingTouch)}");
            public static readonly SkillPrototypeId SpellHolyLight             = new($"Elona.{nameof(SpellHolyLight)}");
            public static readonly SkillPrototypeId SpellVanquishHex           = new($"Elona.{nameof(SpellVanquishHex)}");
            public static readonly SkillPrototypeId SpellTeleport              = new($"Elona.{nameof(SpellTeleport)}");
            public static readonly SkillPrototypeId SpellTeleportOther         = new($"Elona.{nameof(SpellTeleportOther)}");
            public static readonly SkillPrototypeId SpellShortTeleport         = new($"Elona.{nameof(SpellShortTeleport)}");
            public static readonly SkillPrototypeId SpellIdentify              = new($"Elona.{nameof(SpellIdentify)}");
            public static readonly SkillPrototypeId SpellUncurse               = new($"Elona.{nameof(SpellUncurse)}");
            public static readonly SkillPrototypeId SpellOracle                = new($"Elona.{nameof(SpellOracle)}");
            public static readonly SkillPrototypeId SpellMagicDart             = new($"Elona.{nameof(SpellMagicDart)}");
            public static readonly SkillPrototypeId SpellNetherArrow           = new($"Elona.{nameof(SpellNetherArrow)}");
            public static readonly SkillPrototypeId SpellNerveArrow            = new($"Elona.{nameof(SpellNerveArrow)}");
            public static readonly SkillPrototypeId SpellChaosEye              = new($"Elona.{nameof(SpellChaosEye)}");
            public static readonly SkillPrototypeId SpellDarkEye               = new($"Elona.{nameof(SpellDarkEye)}");
            public static readonly SkillPrototypeId SpellIceBolt               = new($"Elona.{nameof(SpellIceBolt)}");
            public static readonly SkillPrototypeId SpellFireBolt              = new($"Elona.{nameof(SpellFireBolt)}");
            public static readonly SkillPrototypeId SpellLightningBolt         = new($"Elona.{nameof(SpellLightningBolt)}");
            public static readonly SkillPrototypeId SpellDarknessBolt          = new($"Elona.{nameof(SpellDarknessBolt)}");
            public static readonly SkillPrototypeId SpellMindBolt              = new($"Elona.{nameof(SpellMindBolt)}");
            public static readonly SkillPrototypeId SpellSummonMonsters        = new($"Elona.{nameof(SpellSummonMonsters)}");
            public static readonly SkillPrototypeId SpellSummonWild            = new($"Elona.{nameof(SpellSummonWild)}");
            public static readonly SkillPrototypeId SpellReturn                = new($"Elona.{nameof(SpellReturn)}");
            public static readonly SkillPrototypeId SpellMagicMap              = new($"Elona.{nameof(SpellMagicMap)}");
            public static readonly SkillPrototypeId SpellSenseObject           = new($"Elona.{nameof(SpellSenseObject)}");
            public static readonly SkillPrototypeId SpellIceBall               = new($"Elona.{nameof(SpellIceBall)}");
            public static readonly SkillPrototypeId SpellFireBall              = new($"Elona.{nameof(SpellFireBall)}");
            public static readonly SkillPrototypeId SpellChaosBall             = new($"Elona.{nameof(SpellChaosBall)}");
            public static readonly SkillPrototypeId SpellRagingRoar            = new($"Elona.{nameof(SpellRagingRoar)}");
            public static readonly SkillPrototypeId SpellDominate              = new($"Elona.{nameof(SpellDominate)}");
            public static readonly SkillPrototypeId SpellWeb                   = new($"Elona.{nameof(SpellWeb)}");
            public static readonly SkillPrototypeId SpellMistOfDarkness        = new($"Elona.{nameof(SpellMistOfDarkness)}");
            public static readonly SkillPrototypeId SpellWallCreation          = new($"Elona.{nameof(SpellWallCreation)}");
            public static readonly SkillPrototypeId SpellRestoreBody           = new($"Elona.{nameof(SpellRestoreBody)}");
            public static readonly SkillPrototypeId SpellRestoreSpirit         = new($"Elona.{nameof(SpellRestoreSpirit)}");
            public static readonly SkillPrototypeId SpellWish                  = new($"Elona.{nameof(SpellWish)}");
            public static readonly SkillPrototypeId SpellBuffHolyShield        = new($"Elona.{nameof(SpellBuffHolyShield)}");
            public static readonly SkillPrototypeId SpellBuffMistOfSilence     = new($"Elona.{nameof(SpellBuffMistOfSilence)}");
            public static readonly SkillPrototypeId SpellBuffRegeneration      = new($"Elona.{nameof(SpellBuffRegeneration)}");
            public static readonly SkillPrototypeId SpellBuffElementalShield   = new($"Elona.{nameof(SpellBuffElementalShield)}");
            public static readonly SkillPrototypeId SpellBuffSpeed             = new($"Elona.{nameof(SpellBuffSpeed)}");
            public static readonly SkillPrototypeId SpellBuffSlow              = new($"Elona.{nameof(SpellBuffSlow)}");
            public static readonly SkillPrototypeId SpellBuffHero              = new($"Elona.{nameof(SpellBuffHero)}");
            public static readonly SkillPrototypeId SpellBuffMistOfFrailness   = new($"Elona.{nameof(SpellBuffMistOfFrailness)}");
            public static readonly SkillPrototypeId SpellBuffElementScar       = new($"Elona.{nameof(SpellBuffElementScar)}");
            public static readonly SkillPrototypeId SpellBuffHolyVeil          = new($"Elona.{nameof(SpellBuffHolyVeil)}");
            public static readonly SkillPrototypeId SpellBuffNightmare         = new($"Elona.{nameof(SpellBuffNightmare)}");
            public static readonly SkillPrototypeId SpellBuffDivineWisdom      = new($"Elona.{nameof(SpellBuffDivineWisdom)}");
            public static readonly SkillPrototypeId SpellMutation              = new($"Elona.{nameof(SpellMutation)}");
            public static readonly SkillPrototypeId SpellAcidGround            = new($"Elona.{nameof(SpellAcidGround)}");
            public static readonly SkillPrototypeId SpellFireWall              = new($"Elona.{nameof(SpellFireWall)}");
            public static readonly SkillPrototypeId SpellDoorCreation          = new($"Elona.{nameof(SpellDoorCreation)}");
            public static readonly SkillPrototypeId SpellBuffIncognito         = new($"Elona.{nameof(SpellBuffIncognito)}");
            public static readonly SkillPrototypeId SpellCrystalSpear          = new($"Elona.{nameof(SpellCrystalSpear)}");
            public static readonly SkillPrototypeId SpellMagicStorm            = new($"Elona.{nameof(SpellMagicStorm)}");
            public static readonly SkillPrototypeId SpellResurrection          = new($"Elona.{nameof(SpellResurrection)}");
            public static readonly SkillPrototypeId SpellBuffContingency       = new($"Elona.{nameof(SpellBuffContingency)}");
            public static readonly SkillPrototypeId SpellFourDimensionalPocket = new($"Elona.{nameof(SpellFourDimensionalPocket)}");
            public static readonly SkillPrototypeId SpellWizardsHarvest        = new($"Elona.{nameof(SpellWizardsHarvest)}");
            public static readonly SkillPrototypeId SpellMeteor                = new($"Elona.{nameof(SpellMeteor)}");
            public static readonly SkillPrototypeId SpellGravity               = new($"Elona.{nameof(SpellGravity)}");

            #endregion

            #region Actions

            public static readonly SkillPrototypeId ActionDrainBlood           = new($"Elona.{nameof(ActionDrainBlood)}");
            public static readonly SkillPrototypeId ActionFireBreath           = new($"Elona.{nameof(ActionFireBreath)}");
            public static readonly SkillPrototypeId ActionColdBreath           = new($"Elona.{nameof(ActionColdBreath)}");
            public static readonly SkillPrototypeId ActionLightningBreath      = new($"Elona.{nameof(ActionLightningBreath)}");
            public static readonly SkillPrototypeId ActionDarknessBreath       = new($"Elona.{nameof(ActionDarknessBreath)}");
            public static readonly SkillPrototypeId ActionChaosBreath          = new($"Elona.{nameof(ActionChaosBreath)}");
            public static readonly SkillPrototypeId ActionSoundBreath          = new($"Elona.{nameof(ActionSoundBreath)}");
            public static readonly SkillPrototypeId ActionNetherBreath         = new($"Elona.{nameof(ActionNetherBreath)}");
            public static readonly SkillPrototypeId ActionNerveBreath          = new($"Elona.{nameof(ActionNerveBreath)}");
            public static readonly SkillPrototypeId ActionPoisonBreath         = new($"Elona.{nameof(ActionPoisonBreath)}");
            public static readonly SkillPrototypeId ActionMindBreath           = new($"Elona.{nameof(ActionMindBreath)}");
            public static readonly SkillPrototypeId ActionPowerBreath          = new($"Elona.{nameof(ActionPowerBreath)}");
            public static readonly SkillPrototypeId ActionTouchOfWeakness      = new($"Elona.{nameof(ActionTouchOfWeakness)}");
            public static readonly SkillPrototypeId ActionTouchOfHunger        = new($"Elona.{nameof(ActionTouchOfHunger)}");
            public static readonly SkillPrototypeId ActionTouchOfPoison        = new($"Elona.{nameof(ActionTouchOfPoison)}");
            public static readonly SkillPrototypeId ActionTouchOfNerve         = new($"Elona.{nameof(ActionTouchOfNerve)}");
            public static readonly SkillPrototypeId ActionTouchOfFear          = new($"Elona.{nameof(ActionTouchOfFear)}");
            public static readonly SkillPrototypeId ActionTouchOfSleep         = new($"Elona.{nameof(ActionTouchOfSleep)}");
            public static readonly SkillPrototypeId ActionShadowStep           = new($"Elona.{nameof(ActionShadowStep)}");
            public static readonly SkillPrototypeId ActionDrawShadow           = new($"Elona.{nameof(ActionDrawShadow)}");
            public static readonly SkillPrototypeId ActionHarvestMana          = new($"Elona.{nameof(ActionHarvestMana)}");
            public static readonly SkillPrototypeId ActionBuffPunishment       = new($"Elona.{nameof(ActionBuffPunishment)}");
            public static readonly SkillPrototypeId ActionPrayerOfJure         = new($"Elona.{nameof(ActionPrayerOfJure)}");
            public static readonly SkillPrototypeId ActionAbsorbMagic          = new($"Elona.{nameof(ActionAbsorbMagic)}");
            public static readonly SkillPrototypeId ActionBuffLulwysTrick      = new($"Elona.{nameof(ActionBuffLulwysTrick)}");
            public static readonly SkillPrototypeId ActionMirror               = new($"Elona.{nameof(ActionMirror)}");
            public static readonly SkillPrototypeId ActionDimensionalMove      = new($"Elona.{nameof(ActionDimensionalMove)}");
            public static readonly SkillPrototypeId ActionChange               = new($"Elona.{nameof(ActionChange)}");
            public static readonly SkillPrototypeId ActionDrawCharge           = new($"Elona.{nameof(ActionDrawCharge)}");
            public static readonly SkillPrototypeId ActionFillCharge           = new($"Elona.{nameof(ActionFillCharge)}");
            public static readonly SkillPrototypeId ActionSwarm                = new($"Elona.{nameof(ActionSwarm)}");
            public static readonly SkillPrototypeId ActionEyeOfMutation        = new($"Elona.{nameof(ActionEyeOfMutation)}");
            public static readonly SkillPrototypeId ActionEyeOfEther           = new($"Elona.{nameof(ActionEyeOfEther)}");
            public static readonly SkillPrototypeId ActionEtherGround          = new($"Elona.{nameof(ActionEtherGround)}");
            public static readonly SkillPrototypeId ActionSuspiciousHand       = new($"Elona.{nameof(ActionSuspiciousHand)}");
            public static readonly SkillPrototypeId ActionEyeOfInsanity        = new($"Elona.{nameof(ActionEyeOfInsanity)}");
            public static readonly SkillPrototypeId ActionRainOfSanity         = new($"Elona.{nameof(ActionRainOfSanity)}");
            public static readonly SkillPrototypeId ActionEyeOfDimness         = new($"Elona.{nameof(ActionEyeOfDimness)}");
            public static readonly SkillPrototypeId ActionSummonCats           = new($"Elona.{nameof(ActionSummonCats)}");
            public static readonly SkillPrototypeId ActionSummonYeek           = new($"Elona.{nameof(ActionSummonYeek)}");
            public static readonly SkillPrototypeId ActionSummonPawn           = new($"Elona.{nameof(ActionSummonPawn)}");
            public static readonly SkillPrototypeId ActionSummonFire           = new($"Elona.{nameof(ActionSummonFire)}");
            public static readonly SkillPrototypeId ActionSummonSister         = new($"Elona.{nameof(ActionSummonSister)}");
            public static readonly SkillPrototypeId ActionSuicideAttack        = new($"Elona.{nameof(ActionSuicideAttack)}");
            public static readonly SkillPrototypeId ActionCurse                = new($"Elona.{nameof(ActionCurse)}");
            public static readonly SkillPrototypeId ActionBuffDeathWord        = new($"Elona.{nameof(ActionBuffDeathWord)}");
            public static readonly SkillPrototypeId ActionBuffBoost            = new($"Elona.{nameof(ActionBuffBoost)}");
            public static readonly SkillPrototypeId ActionInsult               = new($"Elona.{nameof(ActionInsult)}");
            public static readonly SkillPrototypeId ActionDistantAttack4       = new($"Elona.{nameof(ActionDistantAttack4)}");
            public static readonly SkillPrototypeId ActionDistantAttack7       = new($"Elona.{nameof(ActionDistantAttack7)}");
            public static readonly SkillPrototypeId ActionScavenge             = new($"Elona.{nameof(ActionScavenge)}");
            public static readonly SkillPrototypeId ActionEyeOfMana            = new($"Elona.{nameof(ActionEyeOfMana)}");
            public static readonly SkillPrototypeId ActionVanish               = new($"Elona.{nameof(ActionVanish)}");
            public static readonly SkillPrototypeId ActionImpregnate           = new($"Elona.{nameof(ActionImpregnate)}");
            public static readonly SkillPrototypeId ActionGrenade              = new($"Elona.{nameof(ActionGrenade)}");
            public static readonly SkillPrototypeId ActionCheer                = new($"Elona.{nameof(ActionCheer)}");
            public static readonly SkillPrototypeId ActionMewmewmew            = new($"Elona.{nameof(ActionMewmewmew)}");
            public static readonly SkillPrototypeId ActionDecapitation         = new($"Elona.{nameof(ActionDecapitation)}");
            public static readonly SkillPrototypeId ActionDropMine             = new($"Elona.{nameof(ActionDropMine)}");
            public static readonly SkillPrototypeId ActionManisDisassembly     = new($"Elona.{nameof(ActionManisDisassembly)}");

            #endregion

            #pragma warning restore format
        }
    }
}
