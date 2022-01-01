using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Holds HP/MP/stamina and skill data. (Skills and stats are treated the same.)
    /// </summary>
    /// <remarks>
    /// Since max HP/MP/stamina are calculated from stats, I can't really see the benefit of separating the two.
    /// </remarks>
    [RegisterComponent]
    public class SkillsComponent : Component
    {
        public override string Name => "Skills";

        /// <summary>
        /// Current hitpoints.
        /// </summary>
        [DataField]
        public int HP { get; set; }

        /// <summary>
        /// Maximum hitpoints.
        /// </summary>
        [DataField]
        public int MaxHP { get; set; }

        /// <summary>
        /// Current magic points.
        /// </summary>
        [DataField]
        public int MP;

        /// <summary>
        /// Maximum magic points.
        /// </summary>
        [DataField]
        public int MaxMP;

        /// <summary>
        /// Current stamina.
        /// </summary>
        [DataField]
        public int Stamina;

        /// <summary>
        /// Maximum stamina.
        /// </summary>
        [DataField]
        public int MaxStamina;

        /// <summary>
        /// Level, potential and experience for skills and stats.
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, LevelAndPotential> Skills = new();

        /// <summary>
        /// Level, potential, experience and spell stock for spells.
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, LevelPotentialAndStock> Spells = new();
    }

    /// <summary>
    /// The level, potential and experience associated with a skill.
    /// </summary>
    [DataDefinition]
    public class LevelAndPotential
    {
        public const int DEFAULT_POTENTIAL = 100;

        public LevelAndPotential(int level)
        {
            Level = level;
        }

        /// <summary>
        /// Level of the skill.
        /// </summary>
        [DataField]
        public int Level { get; set; } = 1;

        /// <summary>
        /// Potential of the skill, specified as a percentage. 100 is the baseline.
        /// </summary>
        [DataField]
        public int Potential { get; set; } = DEFAULT_POTENTIAL;

        /// <summary>
        /// Current experience of the skill.
        /// </summary>
        /// <remarks>
        /// In Elona, a new skill level is gained per 1000 experience.
        /// </remarks>
        [DataField]
        public int Experience { get; set; } = 0;

        public override string ToString()
        {
            return $"(Level={Level} Potential={Potential}% EXP={Experience})";
        }
    }

    /// <summary>
    /// The level, potential and experience associated with a spell.
    /// </summary>
    [DataDefinition]
    public class LevelPotentialAndStock
    {
        /// <summary>
        /// Level of the spell.
        /// </summary>
        [DataField]
        public int Level { get; set; } = 1;

        /// <summary>
        /// Potential of the spell, specified as a percentage. 100 is the baseline.
        /// </summary>
        [DataField]
        public int Potential { get; set; } = LevelAndPotential.DEFAULT_POTENTIAL;

        /// <summary>
        /// Current experience of the spell.
        /// </summary>
        /// <remarks>
        /// In Elona, a new spell level is gained per 1000 experience.
        /// </remarks>
        [DataField]
        public int Experience { get; set; } = 0;

        /// <summary>
        /// Current spell stock.
        /// </summary>
        [DataField]
        public int SpellStock { get; set; } = 0;

        public override string ToString()
        {
            return $"(Level={Level} Potential={Potential}% EXP={Experience} Stock={SpellStock})";
        }
    }
}
