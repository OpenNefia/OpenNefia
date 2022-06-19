using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Religion
{
    [RegisterComponent]
    public sealed class AltarComponent : Component
    {
        public override string Name => "Altar";

        [DataField]
        public PrototypeId<GodPrototype>? GodID { get; set; }
    }
}
