using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Prototypes;

namespace Why.Core.GameObjects
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
        public int HP;

        /// <summary>
        /// Maximum hitpoints.
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// Current magic points.
        /// </summary>
        public int MP;

        /// <summary>
        /// Maximum magic points.
        /// </summary>
        public int MaxMP;

        /// <summary>
        /// Current stamina.
        /// </summary>
        public int Stamina;

        /// <summary>
        /// Maximum stamina.
        /// </summary>
        public int MaxStamina;

        /// <summary>
        /// Level, potential, and experience for skills and stats.
        /// </summary>
        public Dictionary<PrototypeId<SkillPrototype>, LevelAndPotential> Skills = new();

        /// <summary>
        /// Level, potential, and experience for skills and stats.
        /// </summary>
        public Dictionary<PrototypeId<SkillPrototype>, LevelPotentialAndStock> Spells = new();
    }

    /// <summary>
    /// The level, potential and experience associated with a skill.
    /// </summary>
    [Serializable]
    public class LevelAndPotential
    {
        /// <summary>
        /// Level of the skill.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Potential of the skill, specified as a percentage. 100 is the baseline.
        /// </summary>
        public int Potential { get; set; }

        /// <summary>
        /// Current experience of the skill.
        /// </summary>
        /// <remarks>
        /// In Elona, a new skill level is gained per 1000 experience.
        /// </remarks>
        public int Experience { get; set; }
    }

    /// <summary>
    /// The level, potential and experience associated with a spell.
    /// </summary>
    [Serializable]
    public class LevelPotentialAndStock
    {
        /// <summary>
        /// Level of the spell.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Potential of the spell, specified as a percentage. 100 is the baseline.
        /// </summary>
        public int Potential { get; set; }

        /// <summary>
        /// Current experience of the spell.
        /// </summary>
        /// <remarks>
        /// In Elona, a new spell level is gained per 1000 experience.
        /// </remarks>
        public int Experience { get; set; }

        /// <summary>
        /// Current spell stock.
        /// </summary>
        public int SpellStock { get; set; }
    }
}
