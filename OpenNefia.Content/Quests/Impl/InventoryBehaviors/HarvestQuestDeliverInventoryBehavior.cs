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
using OpenNefia.Content.Weight;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Quests
{
    /// <seealso cref="TradeInventoryBehavior"/>
    public class HarvestQuestDeliverInventoryBehavior : BaseInventoryBehavior
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

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 24));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Put.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.PickUp);

        public QuestTypeHarvestComponent QuestHarvest { get; }

        public HarvestQuestDeliverInventoryBehavior(QuestTypeHarvestComponent questHarvest)
        {
            QuestHarvest = questHarvest;
        }

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new InventoryInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Put.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            // >>>>>>>> shade2/command.hsp:3413 				if iProperty(cnt)!propQuest:continue ..
            return EntityManager.HasComponent<HarvestQuestCropComponent>(item)
                && EntityManager.TryGetComponent<PickableComponent>(item, out var pickable)
                && pickable.OwnState == OwnState.Quest;
            // <<<<<<<< shade2/command.hsp:3413 				if iProperty(cnt)!propQuest:continue ..
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid putItem, int offerAmount)
        {
            // >>>>>>>> shade2/command.hsp:3898 			if invCtrl(1)=0{ ...
            _audio.Play(Protos.Sound.Inv);
            var addWeight = EntityManager.EnsureComponent<WeightComponent>(putItem).Weight.Buffed * _stacks.GetCount(putItem);
            QuestHarvest.CurrentWeight += addWeight;
            _mes.Display(Loc.GetString("Elona.Quest.Types.Harvest.Event.Put",
                ("item", putItem),
                ("addWeight", UiUtils.DisplayWeight(addWeight)),
                ("currentWeight", UiUtils.DisplayWeight(QuestHarvest.CurrentWeight)),
                ("requiredWeight", UiUtils.DisplayWeight(QuestHarvest.RequiredWeight))),
                color: UiColors.MesGreen);

            EntityManager.DeleteEntity(putItem);

            return new InventoryResult.Continuing();
            // <<<<<<<< shade2/command.hsp:3911 				} ...      return "inventory_continue"
        }
    }
}
