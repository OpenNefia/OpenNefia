using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Holds spell level/stock data.
    /// </summary>
    [RegisterComponent]
    public class SpellsComponent : Component
    {
        public override string Name => "Spells";

        /// <summary>
        /// Level, potential, experience and spell stock for spells.
        /// </summary>
        [DataField]
        public SpellHolder Spells { get; } = new();
    }

    [DataDefinition]
    public class SpellHolder : Dictionary<PrototypeId<SkillPrototype>, LevelPotentialAndStock>
    {
        public bool TryGetKnown(PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelPotentialAndStock? level)
        {
            if (!TryGetValue(protoId, out level))
            {
                return false;
            }

            if (level.Stats.Level.Base <= 0)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// The data associated with a spell.
    /// </summary>
    [DataDefinition]
    public class LevelPotentialAndStock
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

        public override string ToString()
        {
            return $"(Level={Stats.Level} Potential={Stats.Potential}% EXP={Stats.Experience} Stock={SpellStock})";
        }
    }
}
