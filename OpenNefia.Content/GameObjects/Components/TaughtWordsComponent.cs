using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects.Components
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TaughtWordsComponent : Component
    {
        [DataField]
        public List<string> TaughtWords { get; set; } = new();
    }
}