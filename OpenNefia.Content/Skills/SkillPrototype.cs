using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Skills
{
    // TODO: make the invidual behaviors for each skill type boolean flags
    // on SkillPrototype instead, and make base skill prototypes.
    public enum SkillType
    {
        /// <summary>
        /// Regular attributes.
        /// </summary>
        Attribute,

        /// <summary>
        /// Regular skills.
        /// </summary>
        Skill,

        /// <summary>
        /// Weapon proficiencies.
        /// </summary>
        WeaponProficiency,

        /// <summary>
        /// Magic.
        /// </summary>
        Spell,

        /// <summary>
        /// Actions.
        /// </summary>
        Action,
        
        /// <summary>
        /// Effects.
        /// </summary>
        Effect,

        /// <summary>
        /// Other skill types.
        /// </summary>
        Other,
    }

    [Prototype("Elona.Skill")]
    public class SkillPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <inheritdoc/>
        [DataField(required: true)]
        public SkillType SkillType { get; } = SkillType.Skill;

        /// <inheritdoc/>
        [DataField]
        public PrototypeId<SkillPrototype>? RelatedSkill { get; }

        /// <summary>
        /// If true, this skill gains experience with each character level gained.
        /// In vanilla, the skills with this flag set are Martial Arts, Bow and Evasion.
        /// </summary>
        [DataField]
        public bool GrowOnLevelUp { get; } = false;

        /// <summary>
        /// Level of this skill to initialize a newly generated character with, regardless of
        /// race/class.
        /// </summary>
        [DataField]
        public int? InitialLevel { get; } = null;
    }
}