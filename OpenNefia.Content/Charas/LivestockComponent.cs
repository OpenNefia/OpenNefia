using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Charas
{
    /// <summary>
    /// Add to an entity to indicate that it is livestock.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class LivestockComponent : Component
    {
    }
}