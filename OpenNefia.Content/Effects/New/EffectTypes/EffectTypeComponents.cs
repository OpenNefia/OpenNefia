using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Effects.New
{
    public enum SpellAlignment
    {
        Positive,
        Neutral,
        Negative
    }

    /// <summary>
    /// Represents spells that can be listed in the casting menu.
    /// They have a casting difficulty and MP cost.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeSpellComponent : Component
    {
        /// <summary>
        /// Spell casting difficulty.
        /// </summary>
        [DataField]
        public int Difficulty { get; set; } = 0;

        /// <summary>
        /// MP cost to cast this spell.
        /// </summary>
        [DataField]
        public int MPCost { get; set; } = 0;

        /// <summary>
        /// Whether this spell has positive or negative effects.
        /// Affects the power of the spell if its corresponding item
        /// is blessed or cursed.
        /// </summary>
        [DataField]
        public SpellAlignment Alignment { get; set; } = SpellAlignment.Positive;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeActionComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeItemComponent : Component
    {
    }
}
