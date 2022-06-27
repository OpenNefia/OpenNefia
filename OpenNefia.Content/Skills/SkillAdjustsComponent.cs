using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Skills
{
    [RegisterComponent]
    public sealed class SkillAdjustsComponent : Component
    {
        public override string Name => "SkillAdjusts";

        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, int> SkillAdjusts { get; } = new();
    }
}
