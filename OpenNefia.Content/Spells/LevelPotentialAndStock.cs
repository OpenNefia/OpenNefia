using OpenNefia.Content.Skills;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.Spells
{
    /// <summary>
    /// The data associated with a spell.
    /// </summary>
    [DataDefinition]
    public class LevelPotentialAndStock : IEquatable<LevelPotentialAndStock>
    {
        public LevelPotentialAndStock() { }

        public LevelPotentialAndStock(int level = 1, int potential = LevelAndPotential.DefaultPotential, int experience = 0, int spellStock = 0)
        {
            Stats = new LevelAndPotential(level, potential, experience);
            SpellStock = spellStock;
        }

        public LevelPotentialAndStock(LevelAndPotential stats, int spellStock = 0)
        {
            Stats = stats;
            SpellStock = spellStock;
        }

        /// <summary>
        /// The level, potential and experience associated with this spell.
        /// </summary>
        [DataField]
        public LevelAndPotential Stats { get; } = new();

        public Stat<int> Level { get => Stats.Level; set => Stats.Level = value; }
        public int Potential { get => Stats.Potential; set => Stats.Potential = value; }
        public int Experience { get => Stats.Experience; set => Stats.Experience = value; }

        /// <summary>
        /// Current spell stock.
        /// </summary>
        [DataField]
        public int SpellStock { get; set; } = 0;

        public bool Equals(LevelPotentialAndStock? other)
        {
            if (other == null)
                return false;

            return Stats.Equals(other.Stats) && SpellStock == other.SpellStock;
        }

        public override string ToString()
        {
            return $"(Level={Stats.Level} Potential={Stats.Potential}% EXP={Stats.Experience} Stock={SpellStock})";
        }
    }
}
