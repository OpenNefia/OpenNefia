using OpenNefia.Content.Currency;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.UI;
using OpenNefia.Content.Weight;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UserInterface;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class SellInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IShopkeeperSystem _shopkeepers = default!;
        [Dependency] private readonly IPickableSystem _pickable = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 12));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Sell.WindowTitle");
        public override bool QueryAmount => true;
        public override bool ShowMoney => true;

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new EntityInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Sell.QueryText");
        }

        public override string GetItemName(InventoryContext context, EntityUid item)
        {
            var weight = EntityManager.GetComponentOrNull<WeightComponent>(item)?.Weight.Buffed ?? 0;
            return base.GetItemName(context, item) + " " + UiUtils.DisplayWeight(weight);
        }

        public override string GetItemDetails(InventoryContext context, EntityUid item)
        {
            var value = _shopkeepers.CalcItemValue(context.User, item, ItemValueMode.Sell);
            return $"{value} gp";
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return _shopkeepers.CanSellItemToShopkeeper(context.User, context.Target, item);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            if (!_pickable.CheckNoDropAndMessage(item))
                return new InventoryResult.Continuing();

            if (!_pickable.CheckPickableOwnStateAndMessage(item))
                return new InventoryResult.Finished(TurnResult.Failed);

            var price = _shopkeepers.CalcItemValue(context.User, item, ItemValueMode.Sell) * amount;

            if (!_playerQuery.YesOrNo(Loc.GetString("Elona.Inventory.Behavior.Sell.PromptConfirm",
                ("item", item),
                ("price", price))))
                return new InventoryResult.Continuing();

            if (!EntityManager.TryGetComponent<MoneyComponent>(context.Target, out var shopkeeperWallet) || price > shopkeeperWallet.Gold)
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Sell.NotEnoughMoney", ("shopkeeper", context.Target)));
                return new InventoryResult.Continuing();
            }

            if (!EntityManager.TryGetComponent<RoleShopkeeperComponent>(context.Target, out var shopkeeper)
                || !_stacks.TrySplit(item, amount, out var split)
                || !shopkeeper.ShopContainer.Insert(split))
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Sell.ShopkeepersInventoryIsFull"));
                return new InventoryResult.Continuing();
            }

            if (!EntityManager.HasComponent<StolenComponent>(item))
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Sell.YouSell.Normal", ("item", item)));
            }
            else
            {
                EntityManager.RemoveComponent<StolenComponent>(item);
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Sell.YouSell.Stolen", ("item", item)));
                // TODO thief guild quest
            }

            _audio.Play(Protos.Sound.Getgold1, context.User);
            if (EntityManager.TryGetComponent<MoneyComponent>(context.User, out var wallet))
                wallet.Gold += price;
            shopkeeperWallet.Gold -= price;

            if (EntityManager.TryGetComponent<PickableComponent>(item, out var pickable))
                pickable.OwnState = OwnState.None;
            if (EntityManager.TryGetComponent<IdentifyComponent>(item, out var identify))
                identify.IdentifyState = IdentifyState.Full;

            return new InventoryResult.Continuing();
        }
    }
}
