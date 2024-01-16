using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Skills
{
    public sealed partial class SkillsSystem
    {
        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateAttributes()
        {
            return _protos.EnumeratePrototypes<SkillPrototype>()
                .Where(skillProto => skillProto.SkillType == SkillType.Attribute);
        }

        /// <summary>
        /// Attributes that are treated differently from the normal set of attributes.
        /// </summary>
        private readonly HashSet<PrototypeId<SkillPrototype>> _nonBaseAttributes = new()
        {
            Skill.AttrSpeed,
            Skill.AttrLuck
        };

        private bool IsBaseAttribute(SkillPrototype skillProto)
        {
            if (skillProto.SkillType != SkillType.Attribute) 
                return false;

            if (_protos.TryGetExtendedData<SkillPrototype, ExtSkillFlags>(skillProto, out var flags) && !flags.IsBaseAttribute)
                return false;

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateBaseAttributes()
        {
            return EnumerateAttributes().Where(IsBaseAttribute);
        }

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateRegularSkills()
        {
            return _protos.EnumeratePrototypes<SkillPrototype>()
                .Where(skillProto => skillProto.SkillType == SkillType.Skill);
        }

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateRegularSkillsAndWeaponProficiencies()
        {
            return _protos.EnumeratePrototypes<SkillPrototype>()
                .Where(skillProto => skillProto.SkillType == SkillType.Skill || skillProto.SkillType == SkillType.WeaponProficiency);
        }

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateWeaponProficiencies()
        {
            return _protos.EnumeratePrototypes<SkillPrototype>()
                .Where(skillProto => skillProto.SkillType == SkillType.WeaponProficiency);
        }

        public SkillPrototype PickRandomAttribute()
        {
            return _rand.Pick(EnumerateAttributes().ToList());
        }

        public SkillPrototype PickRandomBaseAttribute()
        {
            return _rand.Pick(EnumerateBaseAttributes().ToList());
        }

        public SkillPrototype PickRandomRegularSkill()
        {
            return _rand.Pick(EnumerateRegularSkills().ToList());
        }

        public SkillPrototype PickRandomRegularSkillOrWeaponProficiency()
        {
            return _rand.Pick(EnumerateRegularSkillsAndWeaponProficiencies().ToList());
        }
    }

    public sealed class ExtSkillFlags : IPrototypeExtendedData<SkillPrototype>
    {
        /// <summary>
        /// If <c>true</c>, this skill can be improved through wishing.
        /// </summary>
        /// <remarks>
        /// <c>false</c> for: Life, Mana
        /// </remarks>
        [DataField]
        public bool CanWishFor { get; set; } = true;

        /// <summary>
        /// If <c>false</c>, this attribute will not be included in the list of base attributes.
        /// Only applies to skills of type <see cref="SkillType.Attribute"/>.
        /// </summary>
        /// <remarks>
        /// <c>false</c> for: Life, Mana, Speed, Luck
        /// </remarks>
        [DataField]
        public bool IsBaseAttribute { get; set; } = true;
    }
}
