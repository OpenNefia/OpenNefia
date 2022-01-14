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
    [RegisterComponent]
    public class CharaMakeSkillInitTempComponent : Component
    {
        public override string Name => "CharaMakeSkillInitTemp";

        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, int> Skills { get; } = new();
    }
}
