using OpenNefia.Content.Currency;
using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Weight;
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
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IShopkeeperSystem _shopkeepers = default!;
        [Dependency] private readonly IPickableSystem _pickable = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 11));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Buy.WindowTitle");
        public override bool QueryAmount => true;
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

        public override string GetItemName(InventoryContext context, EntityUid item)
        {
            var weight = EntityManager.GetComponentOrNull<WeightComponent>(item)?.Weight ?? 0;
            return base.GetItemName(context, item) + " " + UiUtils.DisplayWeight(weight);
        }

        public override string GetItemDetails(InventoryContext context, EntityUid item)
        {
            var value = _shopkeepers.CalcItemValue(context.User, item, ItemValueMode.Buy);
            return $"{value} gp";
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            if (!_pickable.CheckNoDropAndMessage(item))
                return new InventoryResult.Continuing();

            if (!_pickable.CheckPickableOwnStateAndMessage(item))
                return new InventoryResult.Finished(TurnResult.Failed);

            var cost = _shopkeepers.CalcItemValue(context.User, item, ItemValueMode.Buy) * amount;

            if (!_playerQuery.YesOrNo(Loc.GetString("Elona.Inventory.Behavior.Buy.PromptConfirm",
                ("item", item),
                ("cost", cost))))
                return new InventoryResult.Continuing();

            if (!EntityManager.TryGetComponent<WalletComponent>(context.User, out var wallet) || cost > wallet.Gold)
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Buy.NotEnoughMoney"));
                return new InventoryResult.Continuing();
            }

            if (!EntityManager.TryGetComponent<InventoryComponent>(context.User, out var inv) 
                || !_stacks.TrySplit(item, amount, out var split)
                || !inv.Container.Insert(split))
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Common.InventoryIsFull"));
                return new InventoryResult.Continuing();
            }

            _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Buy.YouBuy", ("item", split)));
            _audio.Play(Protos.Sound.Paygold1, context.User);

            wallet.Gold -= cost;
            if (EntityManager.TryGetComponent<WalletComponent>(context.Target, out var shopkeeperWallet))
                shopkeeperWallet.Gold += cost;

            var ev = new AfterItemPurchasedEvent(context.User, context.Target, amount);
            EntityManager.EventBus.RaiseEvent(split, ev);
            
            _stacks.TryStackAtSamePos(split);

            return new InventoryResult.Continuing();
        }
    }

    public sealed class AfterItemPurchasedEvent : EntityEventArgs
    {
        public EntityUid Buyer { get; }
        public EntityUid Shopkeeper { get; }
        public int AmountBought { get; }

        public AfterItemPurchasedEvent(EntityUid user, EntityUid target, int amount)
        {
            Buyer = user;
            Shopkeeper = target;
            AmountBought = amount;
        }
    }
}
