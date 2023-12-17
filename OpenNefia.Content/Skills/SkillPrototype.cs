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
    public enum SkillType
    {
        /// <summary>
        /// Regular attributes.
        /// </summary>
        Attribute,

        /// <summary>
        /// Special attributes (Life and Mana).
        /// </summary>
        AttributeSpecial,

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

    /// <summary>
    /// <para>
    /// Interface that allows the implementation of "skill-like" leveling for different kinds of
    /// skills, such as regular skills (Mining, Fishing) and magic.
    /// </para>
    /// <para>
    /// This allows skills and magic to share the same potential-based leveling system, while also
    /// allowing the implementation of other special fields in the prototype/instance, like casting
    /// difficulty/spell stock.
    /// </para>
    /// <para>
    /// When using this interface, the actual skill level/potential will typically go in separate
    /// components. For example, the level/potential of skills are held by <see
    /// cref="SkillsComponent"/>, while for spells they're in <see cref="SpellsComponent"/>. The
    /// corresponding entity systems then pass a <see cref="LevelAndPotential"/> to <see
    /// cref="SkillsSystem"/> to do the actual leveling.
    /// </para>
    /// </summary>
    // TODO remove
    public interface ISkillPrototype : IPrototype
    {
        /// <summary>
        /// Type of this skill. Affects things like the level experience divisor, which does not
        /// apply for attribute leveling.
        /// </summary>
        SkillType SkillType { get; }

        /// <summary>
        /// Related skill that gains experience along with this skill.
        /// </summary>
        public PrototypeId<SkillPrototype>? RelatedSkill { get; }
    }

    [Prototype("Elona.Skill")]
    public class SkillPrototype : ISkillPrototype, IHspIds<int>
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
