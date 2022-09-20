using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Areas
{
    /// <summary>
    /// Generates floors using a map blueprint.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class AreaMapBlueprintComponent : Component
    {
        [DataField(required: true)]
        public ResourcePath BlueprintPath { get; set; } = default!;
    }
}