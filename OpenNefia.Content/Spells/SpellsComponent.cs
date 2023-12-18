using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Spells;
using OpenNefia.Core;
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
    public class SpellsComponent : Component, IComponentRefreshable
    {
        /// <summary>
        /// Spell stock for known spells.
        /// </summary>
        /// <remarks>
        /// NOTE: This mechanic only applies to the player in vanilla (and base OpenNefia).
        /// </remarks>
        [DataField]
        public Dictionary<PrototypeId<SpellPrototype>, SpellState> Spells { get; } = new();

        /// <summary>
        /// Text to display when the entity casts a spell. It is purely cosmetic. 
        /// It is an index into <c>Elona.Spells.CastingStyle.<...></c>.
        /// </summary>
        // TODO rework
        [DataField]
        public LocaleKey? CastingStyle { get; set; }

        [DataField]
        public Stat<bool> HasEnhancedSpells { get; set; } = new(false);

        [DataField]
        public Stat<bool> CanCastRapidMagic { get; set; } = new(false);

        public SpellState Ensure(PrototypeId<SpellPrototype> skillID)
        {
            if (!Spells.TryGetValue(skillID, out var spell))
            {
                spell = new SpellState();
                Spells[skillID] = spell;
            }

            return spell;
        }

        public void Refresh()
        {
            HasEnhancedSpells.Reset();
            CanCastRapidMagic.Reset();
        }
    }
}
