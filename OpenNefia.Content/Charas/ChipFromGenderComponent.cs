using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Charas
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ChipFromGenderComponent : Component
    {
        [DataField("chips")]
        private Dictionary<Gender, PrototypeId<ChipPrototype>> _chips = new();
        public IReadOnlyDictionary<Gender, PrototypeId<ChipPrototype>> Chips => _chips;
    }
}