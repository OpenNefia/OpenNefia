using OpenNefia.Content.Logic;
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

        [DataField(required: true)]
        public Quality Quality { get; set; }
    }

    public enum Quality
    {
        Bad = 1 - 1,
        Normal = 2 - 1,
        Good = 3 - 1,
        Great = 4 - 1,
        God = 5 - 1,
        Unique = 6 - 1
    }
}
