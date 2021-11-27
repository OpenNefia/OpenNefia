using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Serialization.Manager.Attributes;

namespace Why.Core.GameObjects
{
    /// <summary>
    /// Holds HP/MP/stamina and skill data. (Skills and stats are treated the same.)
    /// </summary>
    [RegisterComponent]
    public class SkillsComponent : Component
    {
        public override string Name => "Skills";

        [DataField]
        public Dictionary<string, LevelAndPotential> Skills = new();
    }

    /// <summary>
    /// The level, potential and experience associated with a skill/spell.
    /// </summary>
    [Serializable]
    public class LevelAndPotential
    {
        /// <summary>
        /// Level of the skill/spell.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Potential of the skill/spell, specified as a percentage. 100 is the baseline.
        /// </summary>
        public int Potential { get; set; }
        
        /// <summary>
        /// Current experience of the skill/spell. In Elona, a new skill/spell level is gained per 1000 experience.
        /// </summary>
        public int Experience { get; set; }
    }
}
