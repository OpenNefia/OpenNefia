using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Loot
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class LootTypeComponent : Component
    {
        public override string Name => "LootType";

        [DataField]
        public PrototypeId<LootTypePrototype>? LootType { get; set; }

        [DataField("lootDrops")]
        private List<LootDropEntry> _lootDrops = new();
        public IReadOnlyList<LootDropEntry> LootDrops => _lootDrops;
    }
}