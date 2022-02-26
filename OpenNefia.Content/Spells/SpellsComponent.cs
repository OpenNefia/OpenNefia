using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Spells
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
        public Dictionary<PrototypeId<SkillPrototype>, LevelPotentialAndStock> Spells { get; } = new();

        public bool TryGetKnown(PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelPotentialAndStock? level)
        {
            if (!Spells.TryGetValue(protoId, out level))
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
}
