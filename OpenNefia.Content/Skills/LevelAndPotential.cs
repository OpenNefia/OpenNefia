using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Spells;

namespace OpenNefia.Content.Skills
{
    /// <summary>
    /// The level, potential and experience associated with a skill.
    /// </summary>
    [DataDefinition]
    public class LevelAndPotential : IEquatable<LevelAndPotential>
    {
        public const int DefaultPotential = 100;

        public LevelAndPotential() {}

        public LevelAndPotential(int level = 1, int potential = DefaultPotential, int experience = 0)
        {
            Level = new Stat<int>(level);
            Potential = potential;
            Experience = experience;
        }

        /// <summary>
        /// Level of the skill.
        /// </summary>
        [DataField]
        public Stat<int> Level { get; set; } = new(1);

        /// <summary>
        /// Potential of the skill, specified as a percentage. 100 is the baseline.
        /// </summary>
        [DataField]
        public int Potential { get; set; } = DefaultPotential;

        /// <summary>
        /// Current experience of the skill.
        /// </summary>
        /// <remarks>
        /// In Elona, a new skill level is gained per 1000 experience.
        /// </remarks>
        [DataField]
        public int Experience { get; set; } = 0;

        public bool Equals(LevelAndPotential? other)
        {
            if (other == null)
                return false;

            return Level == other.Level
                && Potential == other.Potential
                && Experience == other.Experience;
        }

        public override string ToString()
        {
            return $"(Level={Level} Potential={Potential}% EXP={Experience})";
        }
    }
}
