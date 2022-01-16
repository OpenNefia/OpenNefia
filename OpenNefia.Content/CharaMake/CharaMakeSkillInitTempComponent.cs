using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Overrides the initial skills for a character, which are set during the 
    /// <see cref="EntityGen.EntityGeneratedEvent"/>. Used during character creation for
    /// the skill initialization process there.
    /// </summary>
    [RegisterComponent]
    public class CharaMakeSkillInitTempComponent : Component
    {
        public override string Name => "CharaMakeSkillInitTemp";

        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, int> Skills { get; } = new();
    }
}
