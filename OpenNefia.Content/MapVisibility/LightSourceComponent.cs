using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.MapVisibility
{
    /// <summary>
    /// Indicates that this item emits light.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class LightSourceComponent : Component
    {
        [DataField]
        public PrototypeId<LightSourcePrototype> ID { get; set; }
    }
}