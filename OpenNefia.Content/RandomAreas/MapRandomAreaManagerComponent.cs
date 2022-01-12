using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomAreas
{
    /// <summary>
    /// Indicates that this map can hold random areas. Used for the area entities
    /// of world maps like North Tyris to handle regenerating random areas like Nefia.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapRandomAreaManagerComponent : Component
    {
        public override string Name => "MapRandomAreaManager";

        /// <summary>
        /// If true, the next time this map is entered all random areas will be removed
        /// and regenerated.
        /// </summary>
        [DataField]
        public bool RegenerateRandomAreas { get; set; } = false;
    }
}
