using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Home
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class DeedComponent : Component
    {
        [DataField]
        public PrototypeId<HomePrototype> HomeID { get; set; }
    }
}