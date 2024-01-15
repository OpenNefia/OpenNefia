using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GlobalEntities
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class GlobalEntityComponent : Component
    {
        [DataField]
        public string ID { get; set; } = string.Empty;
    }
}