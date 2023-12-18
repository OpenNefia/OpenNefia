using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// Main component for all effect entities.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectComponent : Component
    {
        /// <summary>
        /// Whether this spell has positive or negative effects.
        /// Affects the power of the effect if its corresponding item
        /// is blessed or cursed.
        /// </summary>
        [DataField]
        public EffectAlignment Alignment { get; set; } = EffectAlignment.Positive;
    }

    public enum EffectAlignment
    {
        Positive,
        Neutral,
        Negative
    }
}
