using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Buffs
{
    [RegisterComponent]
    public class BuffsComponent : Component
    {
        public override string Name => "Buffs";

        [DataField]
        public List<BuffInstance> Buffs { get; } = new();
    }

    [DataDefinition]
    public sealed class BuffInstance
    {
        [DataField]
        public int TurnsRemaining { get; set; } = 0;

        [DataField]
        public PrototypeId<BuffPrototype> BuffID { get; set; }
    }
}
