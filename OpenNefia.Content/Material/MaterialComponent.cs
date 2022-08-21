using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Materials
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MaterialComponent : Component
    {
        public override string Name => "Material";

        [DataField]
        public PrototypeId<MaterialPrototype>? MaterialID { get; set; }

        [DataField]
        public int RandomSeed { get; set; }
    }
}