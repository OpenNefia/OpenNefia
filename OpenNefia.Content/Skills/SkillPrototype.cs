using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
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
        WeaponProficiency
    }

    [Prototype("Elona.Skill")]
    public class SkillPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public PrototypeId<SkillPrototype>? RelatedSkill { get; }

        [DataField(required: true)]
        public SkillType SkillType { get; } = SkillType.Skill;

        [DataField]
        public bool IsPrimarySkill { get; } = false;
    }
}
