using OpenNefia.Content.Pickable;
﻿using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Content.InUse;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.Log;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Oracle;

namespace OpenNefia.Content.Inventory
{
    /// <seealso cref="TradeInventoryBehavior"/>
    public class PresentInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IPickableSystem _pickable = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IShopkeeperSystem _shopkeepers = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IInUseSystem _inUses = default!;
        [Dependency] private readonly IEquipmentSystem _equipment = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IEquipmentGenSystem _equipmentGen = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IOracleSystem _oracle = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 20));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Present.WindowTitle");
        public override bool ApplyNameModifiers => false;
        public override bool QueryAmount => true;

        /// <summary>
        /// Item that is being traded for.
        /// </summary>
        public EntityUid TradeItem { get; }
        
        /// <summary>
        /// Amount of the item that is being traded for.
        /// </summary>
        public int TradeAmount { get; }

        public PresentInventoryBehavior(EntityUid item, int amount)
        {
            TradeItem = item;
            TradeAmount = amount;
        }

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new InventoryInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Present.QueryText", ("item", TradeItem), ("itemAmount", TradeAmount));
        }

        private bool CanTrade(EntityUid player, EntityUid item, int amount)
        {
            var tradeValue = _shopkeepers.CalcItemValue(player, TradeItem, ItemValueMode.Buy) * TradeAmount;
            var offerValue = _shopkeepers.CalcItemValue(player, item, ItemValueMode.Buy) * amount;

            return !_tags.HasTag(item, Protos.Tag.ItemNotrade)
                && offerValue > (tradeValue / 2 * 3)
                && !EntityManager.HasComponent<StolenComponent>(item);
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return CanTrade(context.User, item, _stacks.GetCount(item));
        }

        public override int? OnQueryAmount(InventoryContext context, EntityUid item)
        {
            if (!EntityManager.TryGetComponent<StackComponent>(item, out var stack))
                return null;

            var tradeValue = _shopkeepers.CalcItemValue(context.User, TradeItem, ItemValueMode.Buy) * TradeAmount;
            var offerValueSingle = _shopkeepers.CalcItemValue(context.User, item, ItemValueMode.Buy);

            return Math.Max((tradeValue / 2 * 3) / offerValueSingle + 1, 1);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid offerItem, int offerAmount)
        {
            if (!_pickable.CheckNoDropAndMessage(offerItem)
                || !EntityManager.TryGetComponent<InventoryComponent>(context.User, out var playerInv)
                || !EntityManager.TryGetComponent<InventoryComponent>(context.Target, out var targetInv))
                return new InventoryResult.Continuing();

            if (!CanTrade(context.User, offerItem, offerAmount))
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Present.TooLowValueAmount", ("offerItem", offerItem), ("offerAmount", offerAmount), ("targetItem", TradeItem), ("tradeAmount", TradeAmount)));
                return new InventoryResult.Continuing();
            }

            if (!_playerQuery.YesOrNo(Loc.GetString("Elona.Inventory.Behavior.Present.PromptConfirm",
                ("item", offerItem),
                ("itemAmount", offerAmount),
                ("targetItem", TradeItem),
                ("targetAmount", TradeAmount))))
                return new InventoryResult.Continuing();

            _audio.Play(Protos.Sound.Equip1);
            _inUses.RemoveUserOfItem(TradeItem);
            _inUses.RemoveUserOfItem(offerItem);
            if (EntityManager.HasComponent<AlwaysDropOnDeathComponent>(offerItem))
                EntityManager.RemoveComponent<AlwaysDropOnDeathComponent>(offerItem);

            if (_equipSlots.TryGetSlotEquippedOn(TradeItem, out var owner, out var slot))
            {
                if (!_equipSlots.TryUnequip(owner.Value, slot))
                {
                    Logger.ErrorS("inv.behavior.present", "Could not unequip trade item");
                    _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Present.CannotUnequip", ("owner", owner.Value), ("item", TradeItem)));
                    return new InventoryResult.Continuing();
                }
            }

            _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Present.YouReceive",
                ("player", context.User),
                ("offerItem", offerItem), 
                ("offerAmount", offerAmount),
                ("targetItem", TradeItem),
                ("tradeAmount", TradeAmount)),
              entity: context.User);

            if (_stacks.TrySplit(TradeItem, TradeAmount, out var tradeItemSplit)
                && _stacks.TrySplit(offerItem, offerAmount, out var offerItemSplit))
            {
                if (!playerInv.Container.Insert(tradeItemSplit))
                {
                    Logger.ErrorS("inv.behavior.present", $"Player chara {context.User} failed to take item {tradeItemSplit}");
                    _mes.Display(Loc.GetString("Elona.Common.SomethingFalls.FromBackpack", ("item", tradeItemSplit), ("owner", context.Target)));
                    EntityManager.GetComponent<SpatialComponent>(tradeItemSplit).Coordinates = EntityManager.GetComponent<SpatialComponent>(context.User).Coordinates;
                }
                if (!targetInv.Container.Insert(offerItemSplit))
                {
                    Logger.ErrorS("inv.behavior.present", $"Target chara {context.Target} failed to take item {offerItemSplit}");
                    _mes.Display(Loc.GetString("Elona.Common.SomethingFalls.FromBackpack", ("item", offerItemSplit), ("owner", context.User)));
                    EntityManager.GetComponent<SpatialComponent>(offerItemSplit).Coordinates = EntityManager.GetComponent<SpatialComponent>(context.Target).Coordinates;
                }
            }

            _oracle.ConvertArtifact(ref tradeItemSplit);
            _equipment.EquipAllHighestValueItemsForNPC(context.Target);
            if (!_parties.IsInPlayerParty(context.Target))
                _equipmentGen.GenerateAndEquipEquipment(context.Target);
            _inv.EnsureFreeItemSlot(context.Target);
            _refresh.Refresh(context.Target);

            return new InventoryResult.Finished(TurnResult.Succeeded);
        }

        public override InventoryResult AfterFilter(InventoryContext context, IReadOnlyList<InventoryEntry> filteredItems)
        {
            if (filteredItems.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Present.TooLowValue", ("targetItem", TradeItem), ("tradeAmount", TradeAmount)));
                return new InventoryResult.Finished(TurnResult.Aborted);
            }

            return new InventoryResult.Continuing();
        }
    }
}
