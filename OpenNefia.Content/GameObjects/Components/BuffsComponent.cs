using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class BuffsComponent : Component
    {
        public override string Name => "Buffs";

        [DataField]
        public List<PrototypeId<BuffPrototype>> Buffs { get; } = new();
    }
}
