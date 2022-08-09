using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.UserInterface;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class BuyInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 11));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Buy.WindowTitle");
        public override bool ShowMoney => true;

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            if (EntityManager.TryGetComponent<RoleShopkeeperComponent>(context.Target, out var shopkeeper))
                yield return new ContainerInvSource(shopkeeper.ShopContainer);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Buy.QueryText");
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            _uiManager.Query<ItemDescriptionLayer, EntityUid>(item);

            return new InventoryResult.Continuing();
        }
    }
}
