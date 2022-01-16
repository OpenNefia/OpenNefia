using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
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
        public Stat<Quality> Quality { get; set; } = new(GameObjects.Quality.Bad);
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
