using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Shopkeeper
{
    [Prototype("Elona.ShopInventory")]
    public class ShopInventoryPrototype : IPrototype
    {
        /// <inheritdoc />
        [DataField("id", required: true)]
        public string ID { get; private set; } = default!;
    }
}
