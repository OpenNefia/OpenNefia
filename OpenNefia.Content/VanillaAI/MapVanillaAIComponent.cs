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
    [ComponentUsage(ComponentTarget.Map)]
    public class MapVanillaAIComponent : Component
    {
        public override string Name => "MapVanillaAI";

        /// <summary>
        /// If true, automatically anchors characters generated in this map 
        /// that have the calm AI action <see cref="VanillaAICalmAction.Dull"/>.
        /// </summary>
        [DataField]
        public bool AnchorCitizens { get; set; } = false;

        [DataField]
        public VanillaAICalmAction DefaultCalmAction { get; set; } = VanillaAICalmAction.Roam;

        [DataField]
        public bool NoAggroRefresh { get; set; } = false;
    }
}
