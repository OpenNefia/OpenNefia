using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Cargo
{
    [RegisterComponent]
    public class CargoComponent : Component
    {
        [DataField]
        public int CargoWeight { get; set; } = 0;

        [DataField]
        public int? BuyingPrice { get; set; }
    }
}
