using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.VisualAI.Engine
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class VisualAIComponent : Component
    {
        [DataField]
        public bool Enabled { get; set; } = false;

        [DataField]
        public VisualAIPlan Plan { get; set; } = new();

        [DataField]
        public Dictionary<string, IVisualAITargetValue> StoredTargets { get; set; } = new();
    }
}