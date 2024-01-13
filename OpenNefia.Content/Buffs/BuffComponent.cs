using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Formulae;
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
        /// <summary>
        /// Whether the buff is helpful or harmful (a hex). If the player casts a hex,
        /// the target will be aggroed towards them.
        /// </summary>
        [DataField]
        public BuffAlignment Alignment { get; set; } = BuffAlignment.Positive;

        /// <summary>
        /// Original power provided when this buff was added. Can be modified by
        /// <see cref="BuffSystem.AddBuff(EntityUid, PrototypeId{EntityPrototype}, int, int, EntityUid, BuffsComponent?)"/>.
        /// </summary>
        [DataField]
        public int BasePower { get; set; }

        /// <summary>
        /// General-purpose power of the buff. Has different meanings
        /// depending on the buff type. Also factors into buff resistance
        /// chance, when compared to the target's magic resistance.
        /// </summary>
        [DataField]
        public int Power { get; set; }

        /// <summary>
        /// Time remaining for this buff in turns.
        /// </summary>
        [DataField]
        public int TurnsRemaining { get; set; } = 0;

        /// <summary>
        /// Icon of the buff in the HUD.
        /// </summary>
        [DataField(required: true)]
        public PrototypeId<AssetPrototype> Icon { get; set; }

        /// <summary>
        /// Entity responsible for applying this buff.
        /// </summary>
        [DataField]
        public EntityUid? Source { get; set; }
    }

    /// <summary>
    /// Power and initial duration calculation for buffs.
    /// Variables that can be used:
    /// * basePower - power passed into AddBuff.
    /// </summary>
    /// <remarks>
    /// Buffs like Divine Wisdom calculate power in a special way
    /// since they affect more than one parameter.
    /// </remarks>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffPowerComponent : Component
    {
        /// <summary>
        /// Determines the final duration of the buff in turns.
        /// </summary>
        [DataField]
        public Formula Turns { get; set; } = new("10");

        /// <summary>
        /// Determines the adjusted power of the buff.
        /// </summary>
        [DataField]
        public Formula Power { get; set; } = new("basePower");
    }

    /// <summary>
    /// Causes the buff target to resist having this buff applied to them
    /// based on their magical elemental resistance and the usage of Holy Veil.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffResistableComponent : Component
    {
    }

    /// <summary>
    /// Defines a quality the buff can apply to. Must be combined with <see cref="BuffResistableComponent"/>.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Buff)]
    public sealed class BuffResistableQualityComponent : Component
    {
        /// <summary>
        /// Enemy characters with this quality level or greater will always
        /// resist this buff being applied.
        /// </summary>
        /// <remarks>
        /// Used by Death Word.
        /// </remarks>
        [DataField]
        public Quality ResistQuality { get; set; } = Quality.Bad;
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
        /// If true, the buff will not be automatically healed through status renewal,
        /// where all buffs are normally cleared.
        /// </summary>
        /// <seealso cref="ICharaSystem.RenewStatus(EntityUid, CharaComponent?)"/>
        [DataField]
        public bool NoRemoveOnHeal { get; set; }
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
        /// Plays a curse animation on apply. Removed by Vanquish Hex.
        /// </summary>
        Negative
    }
}