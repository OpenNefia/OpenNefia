using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Cargo
{
    [RegisterComponent]
    public class CargoComponent : Component
    {
        /// <summary>
        /// Cargo weight. Tracked separately from inventory weight.
        /// </summary>
        [DataField]
        public int Weight { get; set; } = 0;

        /// <summary>
        /// Quality of the cargo.
        /// </summary>
        [DataField]
        public int Quality { get; set; } = 0;

        [DataField]
        public int? BuyingPrice { get; set; }
    }
}
