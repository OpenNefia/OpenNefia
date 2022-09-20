using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTrainersComponent : Component
    {
        [DataField]
        public HashSet<PrototypeId<SkillPrototype>> LearnableSkills { get; } = new();
    }
}
