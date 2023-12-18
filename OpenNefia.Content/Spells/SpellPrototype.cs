using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNefia.Content.Effects;

namespace OpenNefia.Content.Spells
{
    /// <summary>
    /// A spell is a skill associated with an effect that can
    /// be learned by a character, has a spell stock, and can appear 
    /// in the Spell menu.
    /// </summary>
    [Prototype("Elona.Spell")]
    public class SpellPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <summary>
        /// Associated skill. If the level is non-zero, the entity
        /// knows this spell. The level of this skill is used
        /// for power/damage calculation.
        /// </summary>
        [DataField(required: true)]
        public PrototypeId<SkillPrototype> SkillID { get; }

        /// <summary>
        /// Effect to invoke when the spell is cast.
        /// </summary>
        [DataField(required: true)]
        public PrototypeId<EntityPrototype> EffectID { get; }

        [DataField]
        public int Difficulty { get; set; } = 0;

        [DataField]
        public int MPCost { get; set; } = 0;

        /// <summary>
        /// Maximum range of the spell in tiles.
        /// </summary>
        [DataField]
        public int MaxRange { get; set; } = 1;

        /// <summary>
        /// If true, MP cost will not scale with the spell's skill level.
        /// </summary>
        [DataField]
        public bool NoMPCostScaling { get; set; } = false;

        [DataField]
        public bool IsRapidCastable { get; set; } = false;
    }
}