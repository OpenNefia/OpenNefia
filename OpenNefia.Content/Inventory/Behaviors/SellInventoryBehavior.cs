using OpenNefia.Content.Shopkeeper;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class SellInventoryBehavior : BaseInventoryBehavior
    {
        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 12));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Sell.WindowTitle");
        public override bool ShowMoney => true;

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            if (EntityManager.TryGetComponent<RoleShopkeeperComponent>(context.Target, out var shopkeeper))
                yield return new ContainerInvSource(shopkeeper.ShopContainer);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Sell.QueryText");
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            return new InventoryResult.Continuing();
        }
    }
}
