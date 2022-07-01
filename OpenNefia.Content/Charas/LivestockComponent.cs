using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Charas
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class LivestockComponent : Component
    {
        public override string Name => "Livestock";
    }
}