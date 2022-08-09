using NetVips;
using OpenNefia.Content.Activity;
using OpenNefia.Content.CharaInfo;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.Weather;
using OpenNefia.Content.World;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
            
            if (_world.State.GameDate >= shopkeeper.RestockDate || true)
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