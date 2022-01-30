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
        AttributeSpecial,
        Attribute,
        WeaponProficiency,
        Skill,
        SkillMagic,
        SkillAction,
        SkillEffect
    }
    [Prototype("Elona.Skill")]
    public class SkillPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField]
        public PrototypeId<SkillPrototype>? RelatedSkill { get; }

        [DataField(required: true)]
        public SkillType SkillType { get; } = SkillType.Skill;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }
    }
}
