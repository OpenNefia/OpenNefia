using OpenNefia.Content.Dialog;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Quests.Impl;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        private void Initialize_Supply()
        {
            SubscribeComponent<QuestTypeSupplyComponent, QuestLocalizeDataEvent>(QuestSupply_Localize);
            SubscribeComponent<QuestTypeSupplyComponent, QuestCalcDifficultyEvent>(QuestSupply_CalcDifficulty);
            SubscribeComponent<QuestTypeSupplyComponent, QuestBeforeGenerateEvent>(QuestSupply_BeforeGenerate);

            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestSupply_AddGiveDialogChoices);
        }

        private void QuestSupply_Localize(EntityUid uid, QuestTypeSupplyComponent component, QuestLocalizeDataEvent args)
        {
            args.OutParams["objective"] = _quests.FormatQuestObjective(_itemName.QualifyNameWithItemType(component.TargetItemID));
        }

        private void QuestSupply_CalcDifficulty(EntityUid uid, QuestTypeSupplyComponent component, QuestCalcDifficultyEvent args)
        {
            var playerLevel = _levels.GetLevel(_gameSession.Player);
            args.OutDifficulty = Math.Clamp(_rand.Next(playerLevel + 5) + 1, 1, 30);
        }

        private void QuestSupply_BeforeGenerate(EntityUid uid, QuestTypeSupplyComponent component, QuestBeforeGenerateEvent args)
        {
            var category = _randomGen.PickTag(component.TargetItemCandidates);
            var itemID = _itemGen.PickRandomItemIdRaw(tags: new[] { category });
            if (itemID == null)
            {
                Logger.ErrorS("quest.supply", $"Failed to generate target quest item with category {category}!");
                args.Cancel();
                return;
            }

            if (TryComp<QuestRewardGoldComponent>(uid, out var questRewardGold))
                questRewardGold.GoldModifier += args.Quest.Difficulty;

            component.TargetItemID = itemID.Value;
        }

        /// <summary>
        /// Add the "give item" choice if the target item is in the player's inventory.
        /// </summary>
        private void QuestSupply_AddGiveDialogChoices(EntityUid uid, QuestClientComponent component, GetDefaultDialogChoicesEvent args)
        {
            foreach (var (quest, questSupply) in _quests.EnumerateQuestsForClient<QuestTypeSupplyComponent>(args.Speaker))
            {
                if (_inv.TryFindItemWithIDInInventory(args.Player, questSupply.TargetItemID, out var item))
                {
                    var choiceExtraData = new DialogQuestSupplyData(quest.Owner, item.Value);
                    args.OutChoices.Add(new()
                    {
                        Text = DialogTextEntry.FromString(Loc.GetString("Elona.Quest.Types.Supply.Dialog.Give", ("item", item.Value))),
                        NextNode = new(Protos.Dialog.QuestSupply, "Give"),
                        ExtraData = new List<IDialogExtraData>() { choiceExtraData }
                    });
                }
            }
        }

        #region Dialog Callbacks

        public sealed class DialogQuestSupplyData : IDialogExtraData
        {
            public DialogQuestSupplyData() {}

            public DialogQuestSupplyData(EntityUid quest, EntityUid item)
            {
                Quest = quest;
                Item = item;
            }

            [DataField]
            public EntityUid Quest { get; }

            [DataField]
            public EntityUid Item { get; }
        }

        public QualifiedDialogNode? QuestSupply_Trade(IDialogEngine engine, IDialogNode node)
        {
            _mes.Display("TODO", UiColors.MesYellow);
            return null;
        }

        public QualifiedDialogNode? QuestSupply_Give(IDialogEngine engine, IDialogNode node)
        {
            var data = engine.Data.Get<DialogQuestSupplyData>();

            _mes.Display(Loc.GetString("Elona.Dialog.Common.YouHandOver", ("player", engine.Player), ("item", data.Item)));

            // TODO AI item to use
            if (_inv.TryGetInventoryContainer(engine.Speaker!.Value, out var inv)
                && _stacks.TrySplit(data.Item, 1, out var split))
            {
                _inv.EnsureFreeItemSlot(engine.Speaker.Value);
                if (!inv.Insert(split, EntityManager))
                {
                    Logger.ErrorS("quest", $"Failed to give quest item {split} to client {engine.Speaker.Value}");
                    _stacks.Use(split, 1);
                }
            }
            else
            {
                _stacks.Use(data.Item, 1);
            }

            var nextNodeID = _quests.TurnInQuest(data.Quest, engine.Speaker.Value);
            return engine.GetNodeByID(nextNodeID);
        }

        #endregion
    }
}