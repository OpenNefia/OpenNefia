using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Prototypes
{
    public enum BuffType
    {
        Blessing,
        Hex,
        Food
    }

    [Prototype("Elona.Buff")]
    public class BuffPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField]
        public string RegionId { get; } = default!;

        [DataField]
        public BuffType BuffType { get; } = BuffType.Blessing;
    }
}
