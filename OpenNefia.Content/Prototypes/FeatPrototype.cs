using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Prototypes
{
    public enum FeatType
    {
        Feat,
        Race,
        Mutation,
        EtherDisease
    }

    [Prototype("Feat")]
    public class FeatPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField]
        public int LevelMin { get; }

        [DataField]
        public int LevelMax { get; }

        [DataField]
        public FeatType FeatType { get; } = FeatType.Feat;
    }
}
