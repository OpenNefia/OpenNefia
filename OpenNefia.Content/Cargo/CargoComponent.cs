using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Cargo
{
    [RegisterComponent]
    public class CargoComponent : Component
    {
        public override string Name => "Cargo";

        [DataField]
        public int CargoWeight { get; set; } = 0;

        [DataField]
        public int? BuyingPrice { get; set; }
    }
}
