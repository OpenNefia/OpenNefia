using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Shopkeeper
{
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1</hspId>
    [RegisterComponent]
    public class RoleShopkeeperComponent : Component
    {
        public static readonly ContainerId ContainerIdShopInventory = new("Elona.ShopInventory");

        /// <inheritdoc />
        public override string Name => "RoleShopkeeper";

        [DataField(required: true)]
        public PrototypeId<ShopInventoryPrototype> InventoryId { get; set; } = default!;

        [DataField]
        public int ShopRank { get; set; } = 1;

        public Container Container { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();

            Container = ContainerHelpers.EnsureContainer<Container>(Owner, ContainerIdShopInventory);
        }
    }
}
