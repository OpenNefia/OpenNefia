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

            #pragma warning restore format
        }
    }
}
