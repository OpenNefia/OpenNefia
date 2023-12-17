using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Spells;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Spells
{
    [DataDefinition]
    public sealed class SpellState
    {
        [DataField]
        public int SpellStock { get; set; } = 0;
    }

    /// <summary>
    /// Holds spell level/stock data.
    /// </summary>
    [RegisterComponent]
    public class SpellsComponent : Component
    {
        /// <summary>
        /// Level, potential, experience and spell stock for spells.
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<SpellPrototype>, SpellState> Spells { get; } = new();

        public SpellState Ensure(PrototypeId<SpellPrototype> skillID)
        {
            if (!Spells.TryGetValue(skillID, out var spell))
            {
                spell = new SpellState();
                Spells[skillID] = spell;
            }

            return spell;
        }
    }
}
