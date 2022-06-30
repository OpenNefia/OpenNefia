using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Buffs
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

        ///<summary> 
        /// Asset region ID for use with <see cref="Protos.Asset.BuffIcons"/>. 
        ///</summary>
        [DataField]
        public string RegionId { get; } = default!;

        [DataField]
        public BuffType BuffType { get; } = BuffType.Blessing;
    }
}
