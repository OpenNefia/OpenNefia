using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Indicates this character should be treated like a temporary ally (Poppy), meaning certain
    /// interaction options/behaviors normally available to allies should be disabled.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EscortComponent : Component
    {
        public override string Name => "Escort";
    }
}