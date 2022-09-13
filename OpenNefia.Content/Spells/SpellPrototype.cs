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
    [Prototype("Elona.Spell")]
    public class SpellPrototype : ISkillPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <inheritdoc/>
        [DataField]
        public SkillType SkillType { get; } = SkillType.Spell;

        /// <inheritdoc/>
        [DataField]
        public PrototypeId<SkillPrototype>? RelatedSkill { get; }

        /// <summary>
        /// Difficulty of casting this spell/using this action.
        /// </summary>
        [DataField]
        public int Difficulty { get; }

        /// <summary>
        /// Range in tiles of this spell/action.
        /// </summary>
        [DataField]
        public int Range { get; } = 1;
        
        [DataField]
        public SpellAlignment Alignment { get; } = SpellAlignment.Positive;
        
        [DataField(required: true)]
        public IEffect Effect { get; } = new NullEffect();
    }
    
    public enum SpellAlignment
    {
        Positive,
        Negative
    }
}