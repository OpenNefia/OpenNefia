using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class ItemComponent : Component
    {
        public override string Name => "Item";

        [DataField]
        public int Value { get; set; } = 0;

        [DataField]
        public int IdentifyDifficulty { get; set; } = 0;

        [DataField]
        public PrototypeId<MaterialPrototype>? Material { get; set; }

        [DataField("originalnameref2")]
        public string? OriginalNameRef2 { get; set; }
    }
}
