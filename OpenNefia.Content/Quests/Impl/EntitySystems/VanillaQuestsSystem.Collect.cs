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
using OpenNefia.Core.IoC;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Random;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Loot;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        private void Initialize_Collect()
        {
            SubscribeComponent<QuestTypeCollectComponent, QuestLocalizeDataEvent>(QuestCollect_Localize);
            SubscribeComponent<QuestTypeCollectComponent, QuestCalcDifficultyEvent>(QuestCollect_CalcDifficulty);
            SubscribeComponent<QuestTypeCollectComponent, QuestBeforeGenerateEvent>(QuestCollect_BeforeGenerate);
            SubscribeComponent<QuestTypeCollectComponent, QuestGetTargetCharasEvent>(QuestCollect_GetTargetCharas);

            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestCollect_AddGiveDialogChoices);
            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestCollect_AddTradeDialogChoice);
        }

        private void QuestCollect_Localize(EntityUid uid, QuestTypeCollectComponent collectQuest, QuestLocalizeDataEvent args)
        {
            args.OutParams["itemName"] = _quests.FormatQuestObjective(_itemName.QualifyNameWithItemType(collectQuest.TargetItemID));

            string targetName;
            if (collectQuest.TargetCharaName != null)
                targetName = collectQuest.TargetCharaName;
            else
                targetName = Loc.GetString("Elona.Quest.Types.Collect.TargetIn", ("mapName", args.Quest.ClientOriginatingMapName));

            args.OutParams["targetName"] = targetName;
        }

        private void QuestCollect_CalcDifficulty(EntityUid uid, QuestTypeCollectComponent collectQuest, QuestCalcDifficultyEvent args)
        {
            var playerLevel = _levels.GetLevel(_gameSession.Player);
            args.OutDifficulty = playerLevel / 3;
        }

        private bool CanBeCollectQuestTargetChara(EntityUid uid, CharaComponent? chara = null)
        {
            if (!Resolve(uid, ref chara))
                return false;

            return _factions.GetRelationToPlayer(uid) == Factions.Relation.Neutral
                && (HasComp<RoleGuardComponent>(uid) || HasComp<RoleCitizenComponent>(uid));
        }

        private void QuestCollect_BeforeGenerate(EntityUid uid, QuestTypeCollectComponent collectQuest, QuestBeforeGenerateEvent args)
        {
            if (!TryMap(args.Quest.ClientEntity, out var map))
            {
                args.Cancel();
                return;
            }

            var candidates = _charas.EnumerateNonAllies(map).ToList();
            _rand.Shuffle(candidates);

            foreach (var chara in candidates)
            {
                if (chara.Owner != args.Quest.ClientEntity)
                {
                    if (CanBeCollectQuestTargetChara(chara.Owner, chara) && TryComp<InventoryComponent>(chara.Owner, out var inv))
                    {
                        var category = _randomGen.PickTag(Protos.TagSet.ItemCollect);
                        var itemFilter = new ItemFilter()
                        {
                            MinLevel = 40,
                            Quality = Qualities.Quality.Normal,
                            Tags = new[] { category }
                        };
                        var item = _itemGen.GenerateItem(inv.Container, itemFilter);
                        if (IsAlive(item) && TryProtoID(item.Value, out var protoID))
                        {
                            EnsureComp<AlwaysDropOnDeathComponent>(item.Value);
                            collectQuest.TargetChara = chara.Owner;
                            collectQuest.TargetCharaName = _displayNames.GetDisplayName(chara.Owner);
                            collectQuest.TargetItemID = protoID.Value;
                            break;
                        }
                    }
                }
            }

            if (!IsAlive(collectQuest.TargetChara))
            {
                Logger.ErrorS("quest.collect", "No target character found in map.");
                args.Cancel();
                return;
            }
        }

        /// <summary>
        /// Adds the "give item" choice if the target item is in the player's inventory.
        /// </summary>
        private void QuestCollect_AddGiveDialogChoices(EntityUid questUid, QuestClientComponent collectQuest, GetDefaultDialogChoicesEvent args)
        {
            foreach (var (quest, questCollect) in _quests.EnumerateAllQuestsForClient<QuestTypeCollectComponent>(args.Speaker))
            {
                if (_inv.TryFindItemWithIDInInventory(args.Player, questCollect.TargetItemID, out var item))
                {
                    var choiceExtraData = new DialogQuestGiveItemData(quest.Owner, item.Value);
                    args.OutChoices.Add(new()
                    {
                        Text = DialogTextEntry.FromString(Loc.GetString("Elona.Quest.Dialog.Choices.Give", ("item", item.Value))),
                        NextNode = new(Protos.Dialog.QuestCommon, "Give"),
                        ExtraData = new List<IDialogExtraData>() { choiceExtraData }
                    });
                }
            }
        }

        /// <summary>
        /// Adds the "trade" choice if the target item is in the speaker's inventory and they are the quest's target character.
        /// </summary>
        private void QuestCollect_AddTradeDialogChoice(EntityUid questUid, QuestClientComponent collectQuest, GetDefaultDialogChoicesEvent args)
        {
            bool found = _quests.EnumerateAcceptedQuests<QuestTypeCollectComponent>()
                .Where(pair => pair.QuestType.TargetChara == args.Speaker)
                .Any(pair => _inv.TryFindItemWithIDInInventory(args.Speaker, pair.QuestType.TargetItemID, out _));

            if (found)
            {
                args.OutChoices.Add(new()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Villager.Choices.Trade"),
                    NextNode = new(Protos.Dialog.QuestCommon, "Trade"),
                });
            }
        }

        private void QuestCollect_GetTargetCharas(EntityUid questUid, QuestTypeCollectComponent collectQuest, QuestGetTargetCharasEvent args)
        {
            args.OutTargetCharas.Add(collectQuest.TargetChara);
        }
    }
}