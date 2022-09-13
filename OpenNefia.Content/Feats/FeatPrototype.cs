using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Feats
{
    public enum FeatType
    {
        Feat = 0,
        Mutation = 1,
        Race = 2,
        EtherDisease = 3
    }

    [Prototype("Elona.Feat")]
    public class FeatPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public int LevelMin { get; }

        [DataField]
        public int LevelMax { get; }

        [DataField]
        public FeatType FeatType { get; } = FeatType.Feat;
    }
}
