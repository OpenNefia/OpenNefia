using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Book
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TextbookComponent : Component
    {
        [DataField]
        public PrototypeId<SkillPrototype> SkillID { get; set; }
    }
}