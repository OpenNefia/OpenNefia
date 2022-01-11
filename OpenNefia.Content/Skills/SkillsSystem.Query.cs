using OpenNefia.Core.Prototypes;
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
                .Where(skillProto => skillProto.SkillType == SkillType.Stat);
        }

        /// <summary>
        /// Attributes that are treated differently from the normal set of attributes.
        /// </summary>
        private readonly HashSet<PrototypeId<SkillPrototype>> _nonBaseAttributes = new()
        {
            Skill.StatSpeed,
            Skill.StatLuck
        };

        /// <inheritdoc/>
        public IEnumerable<SkillPrototype> EnumerateBaseAttributes()
        {
            return EnumerateAllAttributes()
                .Where(skill => !_nonBaseAttributes.Contains(skill.GetStrongID()));
        }
    }
}
