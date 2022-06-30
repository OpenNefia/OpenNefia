using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
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
        public IEnumerable<SkillPrototype> EnumerateAllAttributes()
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

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateBaseAttributes()
        {
            return EnumerateAllAttributes()
                .Where(skill => !_nonBaseAttributes.Contains(skill.GetStrongID()));
        }

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateRegularSkills()
        {
            return _protos.EnumeratePrototypes<SkillPrototype>()
                .Where(skillProto => skillProto.SkillType == SkillType.Skill);
        }

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateWeaponProficiencies()
        {
            return _protos.EnumeratePrototypes<SkillPrototype>()
                .Where(skillProto => skillProto.SkillType == SkillType.WeaponProficiency);
        }

        public SkillPrototype PickRandomBaseAttribute()
        {
            return _rand.Pick(EnumerateBaseAttributes().ToList());
        }
    }
}
