using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.VanillaAI
{
    [RegisterComponent]
    public class MapVanillaAIComponent : Component
    {
        public override string Name => "MapVanillaAI";

        [DataField]
        public bool AnchorCitizens { get; set; } = false;

        [DataField]
        public VanillaAICalmAction DefaultCalmAction { get; set; } = VanillaAICalmAction.None;
    }
}
