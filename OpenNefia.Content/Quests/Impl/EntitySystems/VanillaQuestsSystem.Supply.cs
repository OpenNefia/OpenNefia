using OpenNefia.Content.Dialog;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
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
            args.OutParams["itemName"] = _quests.FormatQuestObjective(_itemName.QualifyNameWithItemType(component.TargetItemID));
            args.OutDetailLocaleKey = "Elona.Quest.Types.Supply.Detail";
        }

        private void QuestSupply_CalcDifficulty(EntityUid uid, QuestTypeSupplyComponent component, QuestCalcDifficultyEvent args)
        {
            var playerLevel = _levels.GetLevel(_gameSession.Player);
            args.OutDifficulty = Math.Clamp(_rand.Next(playerLevel + 5) + 1, 1, 30);
        }

        private void QuestSupply_BeforeGenerate(EntityUid uid, QuestTypeSupplyComponent component, QuestBeforeGenerateEvent args)
        {
            var category = _randomGen.PickTag(Protos.TagSet.ItemSupply);
            var itemID = _itemGen.PickRandomItemIdRaw(args.Map, tags: new[] { category });
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
            foreach (var (quest, questSupply) in _quests.EnumerateAllQuestsForClient<QuestTypeSupplyComponent>(args.Speaker))
            {
                if (_inv.TryFindItemWithIDInInventory(args.Player, questSupply.TargetItemID, out var item))
                {
                    var choiceExtraData = new DialogQuestGiveItemData(quest.Owner, item.Value);
                    choiceExtraData.CheckPoison = true;
                    args.OutChoices.Add(new()
                    {
                        Text = DialogTextEntry.FromString(Loc.GetString("Elona.Quest.Dialog.Choices.Give", ("item", item.Value))),
                        NextNode = new(Protos.Dialog.QuestCommon, "Give"),
                        ExtraData = new List<IDialogExtraData>() { choiceExtraData }
                    });
                }
            }
        }
    }
}