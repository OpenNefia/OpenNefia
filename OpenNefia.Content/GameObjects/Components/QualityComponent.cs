using OpenNefia.Content.Logic;
using OpenNefia.Content.Stats;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class QualityComponent : Component
    {
        public override string Name => "Quality";

        // [DataField(required: true)]
        public ValueStat<Quality> Quality { get; set; }
    }

    public enum Quality
    {
        Bad = 0,
        Normal = 1,
        Good = 2,
        Great = 3,
        God = 4,
        Unique = 5
    }
}
