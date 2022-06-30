using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Marks an entity as a map object.
    /// Used for checking the existence of other map object-like things on the same tile.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MObjComponent : Component
    {
        public override string Name => "MObj";
    }
}