using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items.Impl
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MonsterBallComponent : Component
    {
        [DataField]
        public int MaxLevel { get; set; }

        [DataField]
        public PrototypeId<EntityPrototype>? CapturedEntityID { get; set; }

        [DataField]
        public int CapturedEntityLevel { get; set; }
    }
}