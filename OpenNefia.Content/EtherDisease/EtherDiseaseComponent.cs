using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.EtherDisease
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EtherDiseaseComponent : Component
    {
        public override string Name => "EtherDisease";

        [DataField]
        public int Corruption { get; set; } = 0;
    }
}