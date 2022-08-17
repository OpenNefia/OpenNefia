using OpenNefia.Content.Inventory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem : EntitySystem
    {
        [Dependency] private readonly IShopkeeperSystem _shopkeepers = default!;

        private void Shopkeeper_Initialize()
        {
            SubscribeComponent<RoleShopkeeperComponent, GetDefaultDialogChoicesEvent>(Shopkeeper_AddDialogChoices, priority: EventPriorities.High);
        }

        private void Shopkeeper_AddDialogChoices(EntityUid uid, RoleShopkeeperComponent component, GetDefaultDialogChoicesEvent args)
        {
            args.OutChoices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Shopkeeper.Choices.Buy"),
                NextNode = new(Protos.Dialog.Shopkeeper, "Buy")
            });
            args.OutChoices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Shopkeeper.Choices.Sell"),
                NextNode = new(Protos.Dialog.Shopkeeper, "Sell")
            });
        }

        public void Shopkeeper_Buy(IDialogEngine engine, IDialogNode node)
        {
            if (!TryComp<RoleShopkeeperComponent>(engine.Speaker, out var shopkeeper))
                return;
            
            if (_world.State.GameDate >= shopkeeper.RestockDate)
            {
                _shopkeepers.RestockShop(engine.Speaker.Value, shopkeeper);
            }

            var context = new InventoryContext(engine.Player, engine.Speaker.Value, new BuyInventoryBehavior());
            _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);
        }

        public void Shopkeeper_Sell(IDialogEngine engine, IDialogNode node)
        {
            var context = new InventoryContext(engine.Player, engine.Speaker!.Value, new SellInventoryBehavior());
            _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);
        }
    }
}