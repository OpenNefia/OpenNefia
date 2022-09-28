using OpenNefia.Content.Dialog;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Areas;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly IAreaKnownEntrancesSystem _areaKnownEntrances = default!;

        private void Initialize_Deliver()
        {
            SubscribeComponent<QuestTypeDeliverComponent, QuestLocalizeDataEvent>(QuestDeliver_Localize);
            SubscribeComponent<QuestTypeDeliverComponent, QuestCalcDifficultyEvent>(QuestDeliver_CalcDifficulty);
            SubscribeComponent<QuestTypeDeliverComponent, QuestCalcRewardsEvent>(QuestDeliver_CalcRewards);
            SubscribeComponent<QuestTypeDeliverComponent, QuestBeforeGenerateEvent>(QuestDeliver_BeforeGenerate);
            SubscribeComponent<QuestTypeDeliverComponent, QuestBeforeAcceptEvent>(QuestDeliver_BeforeAccept);

            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestDeliver_AddGiveDialogChoices);
        }

        private void QuestDeliver_Localize(EntityUid uid, QuestTypeDeliverComponent questDeliver, QuestLocalizeDataEvent args)
        {
            args.OutParams["itemName"] = _quests.FormatQuestObjective(_itemName.QualifyNameWithItemType(questDeliver.TargetItemID));
            args.OutParams["itemCategory"] = Loc.GetPrototypeString(questDeliver.TargetItemCategory, "Name");
            args.OutParams["targetCharaName"] = questDeliver.TargetCharaName;
            args.OutParams["targetMapName"] = questDeliver.TargetMapName;
        }

        private void QuestDeliver_CalcDifficulty(EntityUid uid, QuestTypeDeliverComponent component, QuestCalcDifficultyEvent args)
        {
            var rewardGold = EnsureComp<QuestRewardGoldComponent>(uid);
            args.OutDifficulty = Math.Clamp(rewardGold.GoldModifier / 20, 1, 25);
        }

        private void QuestDeliver_CalcRewards(EntityUid uid, QuestTypeDeliverComponent component, QuestCalcRewardsEvent args)
        {
            // >>>>>>>> shade2/quest.hsp:458 	if qExist(rq)=qDeliver : p=rnd(2)+1:else:p=1 ..
            args.OutPlatinum = _rand.Next(2) + 1;
            // <<<<<<<< shade2/quest.hsp:458 	if qExist(rq)=qDeliver : p=rnd(2)+1:else:p=1 ..
        }

        private bool TryGetClosestEntranceInMap(MapCoordinates parentMapCoords, MapId destMapID, [NotNullWhen(true)] out AreaEntranceMetadata? result)
        {
            result = _areaKnownEntrances.EnumerateKnownEntrancesTo(destMapID)
                .Where(e => e.MapCoordinates.MapId == parentMapCoords.MapId)
                .MinBy(e =>
                {
                    if (!parentMapCoords.TryDistanceFractional(e.MapCoordinates, out var dist))
                        return float.MaxValue;

                    return dist;
                });
            return result != null;
        }

        private void QuestDeliver_BeforeGenerate(EntityUid uid, QuestTypeDeliverComponent questDeliver, QuestBeforeGenerateEvent args)
        {
            bool IsAlreadyDeliverQuestTarget(EntityUid client)
            {
                return _quests.EnumerateAllQuests<QuestTypeDeliverComponent>()
                    .Any(pair => pair.QuestType.TargetChara == client);
            }

            bool CanBeDeliverQuestTarget(MapId mapID, EntityUid client)
            {
                if (mapID == args.Quest.ClientOriginatingMapID)
                    return false;

                if (IsAlreadyDeliverQuestTarget(client))
                    return false;

                return true;
            }

            var candidates = _quests.EnumerateQuestHubs()
                .SelectMany(t => t.QuestHub.Clients.Values.Select(c => (t.Area, t.MapId, t.QuestHub, c)))
                .Where(p => CanBeDeliverQuestTarget(p.MapId, p.c.ClientEntityUid))
                .ToList();
            _rand.Shuffle(candidates);

            IArea? sourceArea = null;
            MapCoordinates? sourceCoords = null;
            IArea? destArea = null;
            AreaEntranceMetadata? destEntrance = null;
            MapId destMapId;
            QuestHubData destQuestHub;
            QuestClient destClient;

            var found = false;
            foreach (var cand in candidates)
            {
                (destArea, destMapId, destQuestHub, destClient) = cand;

                if (!TryComp<MapEdgesEntranceComponent>(args.Map.MapEntityUid, out var mapEdges))
                {
                    Logger.WarningS("quest.deliver", $"Can't find parent entrance location in map {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName})");
                    args.Cancel();
                    return;
                }

                if (!mapEdges.Entrance.TryGetMapCoordinates(_gameSession.Player, args.Map, out sourceCoords)
                    || !TryGetClosestEntranceInMap(sourceCoords.Value, destMapId, out destEntrance))
                {
                    Logger.WarningS("quest.deliver", $"Can't find entrance to map {destMapId} from map {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName})");
                    args.Cancel();
                    return;
                }

                questDeliver.TargetMapID = destMapId;
                questDeliver.TargetMapName = destQuestHub.MapName;
                questDeliver.TargetChara = destClient.ClientEntityUid;
                questDeliver.TargetCharaName = destClient.ClientName;
                found = true;
                break;
            }

            if (!found)
            {
                Logger.WarningS("quest.deliver", $"No deliver client found outside map {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName})");
                args.Cancel();
                return;
            }

            var itemCategory = _randomGen.PickTag(Protos.TagSet.ItemDeliver);
            var itemID = _itemGen.PickRandomItemIdRaw(tags: new[] { questDeliver.TargetItemCategory });
            if (itemID == null)
            {
                Logger.WarningS("quest.deliver", $"No valid item ID found for category {questDeliver.TargetItemCategory} - {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName})");
                args.Cancel();
                return;
            }

            questDeliver.TargetItemCategory = itemCategory;
            questDeliver.TargetItemID = itemID.Value;
            args.Quest.LocaleKeyRoot = new LocaleKey($"Elona.Quest.Types.Deliver.Categories.{itemCategory}.Variants");

            var distance = 0f;
            if (sourceCoords!.Value.TryDistanceTiled(destEntrance!.MapCoordinates, out var entranceDist))
                distance = entranceDist;

            var rewardGold = EnsureComp<QuestRewardGoldComponent>(uid);
            rewardGold.GoldModifier = 70 + entranceDist * 2;
            if (AreaIsNoyel(sourceArea!) || AreaIsNoyel(destArea!)) 
            {
                rewardGold.GoldModifier = (int)(rewardGold.GoldModifier * 1.75f);
            }

            if (itemCategory == Protos.Tag.ItemCatSpellbook)
            {
                EnsureComp<QuestRewardRandomCategoryComponent>(uid).ItemCategories = Protos.TagSet.ItemMagic;
            }
            else if (itemCategory == Protos.Tag.ItemCatOre)
            {
                EnsureComp<QuestRewardRandomCategoryComponent>(uid).ItemCategories = Protos.TagSet.ItemArmor;
            }
            else if (itemCategory == Protos.Tag.ItemCatJunk)
            {
                EnsureComp<QuestRewardSingleCategoryComponent>(uid).ItemCategory = Protos.Tag.ItemCatOre;
            }
            else if (itemCategory == Protos.Tag.ItemCatFurniture)
            {
                EnsureComp<QuestRewardSingleCategoryComponent>(uid).ItemCategory = Protos.Tag.ItemCatFurniture;
            }
        }

        private void QuestDeliver_BeforeAccept(EntityUid uid, QuestTypeDeliverComponent questDeliver, QuestBeforeAcceptEvent args)
        {
            if (!TryComp<InventoryComponent>(_gameSession.Player, out var inv))
            {
                args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestDeliver, "Accept_BackpackIsFull");
                args.Cancel();
                return;
            }

            var item = _itemGen.GenerateItem(inv.Container, questDeliver.TargetItemID);
            if (!IsAlive(item))
            {
                args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestDeliver, "Accept_BackpackIsFull");
                args.Cancel();
                return;
            }

            _mes.Display(Loc.GetString("Elona.Common.PutInBackpack", ("item", item.Value)));
            _audio.Play(Protos.Sound.Inv, _gameSession.Player);
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestDeliver, "Accept");
        }

        private bool AreaIsNoyel(IArea area)
        {
            return TryProtoID(area!.AreaEntityUid, out var protoID) && protoID.Value == Protos.Area.Noyel;
        }

        /// <summary>
        /// Add the "give item" choice if the target item is in the player's inventory.
        /// </summary>
        private void QuestDeliver_AddGiveDialogChoices(EntityUid uid, QuestClientComponent component, GetDefaultDialogChoicesEvent args)
        {
            foreach (var (quest, questDeliver) in _quests.EnumerateAllQuests<QuestTypeDeliverComponent>()
                .Where(q => q.QuestType.TargetChara == args.Speaker))
            {
                if (_inv.TryFindItemWithIDInInventory(args.Player, questDeliver.TargetItemID, out var item))
                {
                    var choiceExtraData = new DialogQuestGiveItemData(quest.Owner, item.Value);
                    args.OutChoices.Add(new()
                    {
                        Text = DialogTextEntry.FromString(Loc.GetString("Elona.Quest.Dialog.Choices.Deliver", ("item", item.Value))),
                        NextNode = new(Protos.Dialog.QuestCommon, "Give"),
                        ExtraData = new List<IDialogExtraData>() { choiceExtraData }
                    });
                }
            }
        }
    }
}