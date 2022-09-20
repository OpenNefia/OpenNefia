using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.FruitTree
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class FruitTreeComponent : Component
    {
        [DataField]
        public int FruitAmount { get; set; }

        [DataField]
        public PrototypeId<EntityPrototype> FruitItemID { get; set; }
    }
}