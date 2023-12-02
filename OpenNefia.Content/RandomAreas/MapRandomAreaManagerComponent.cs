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
        /// <summary>
        /// If true, the next time this map is entered all random areas will be removed
        /// and regenerated.
        /// </summary>
        [DataField]
        public bool AboutToRegenerateRandomAreas { get; set; } = false;

        /// <summary>
        /// Number of random areas generated when this world is refreshed.
        /// </summary>
        [DataField]
        public int RandomAreaGenerateCount { get; set; } = 40;

        /// <summary>
        /// The number of active random areas that should exist in this world map at any given time.
        /// If the live number drops below this amount, then enough new random areas will be generated
        /// to fill the needed amount.
        /// </summary>
        [DataField]
        public int RandomAreaMinCount { get; set; } = 25;
    }
}
