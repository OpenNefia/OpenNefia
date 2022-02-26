using OpenNefia.Content.Skills;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Spells
{
    /// <summary>
    /// The data associated with a spell.
    /// </summary>
    [DataDefinition]
    public class LevelPotentialAndStock : IEquatable<LevelPotentialAndStock>
    {
        /// <summary>
        /// The level, potential and experience associated with this spell.
        /// </summary>
        [DataField]
        public LevelAndPotential Stats { get; } = new();

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
