using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Buffs
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffComponent : Component
    {
        [DataField]
        public BuffAlignment Alignment { get; set; } = BuffAlignment.Positive;

        [DataField]
        public int Power { get; set; }

        [DataField]
        public GameTimeSpan TimeRemaining { get; set; } = GameTimeSpan.Zero;

        [DataField(required: true)]
        public PrototypeId<AssetPrototype> Icon { get; set; }
    }

    /// <summary>
    /// Causes this buff to resist removal when Holy Light/Vanquish Hex are used.
    /// Buffs without this component will not be affected by hex removal spells.
    /// Standard for all negative alignment buffs (hexes).
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffResistRemovalComponent : Component
    {
        /// <summary>
        /// If true, the buff will not be automatically healed through various means.
        /// </summary>
        [DataField]
        public bool NoRemoveOnHeal { get; set; }
    }

    /// <summary>
    /// Shared between all food buffs.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffFoodComponent : Component
    {
    }

    public enum BuffAlignment
    {
        /// <summary>
        /// Plays a blessing animation on apply.
        /// </summary>
        Positive,

        /// <summary>
        /// No special effects (food buffs).
        /// </summary>
        Neutral,
        
        /// <summary>
        /// Plays a curse animation on apply.
        /// </summary>
        Negative
    }
}