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
using OpenNefia.Core.Maths;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Fame;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly IAreaKnownEntrancesSystem _areaKnownEntrances = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IKarmaSystem _karma = default!;

        private void Initialize_Deliver()
        {
            SubscribeComponent<QuestTypeDeliverComponent, QuestLocalizeDataEvent>(QuestDeliver_Localize);
            SubscribeComponent<QuestTypeDeliverComponent, QuestCalcDifficultyEvent>(QuestDeliver_CalcDifficulty);
            SubscribeComponent<QuestTypeDeliverComponent, QuestCalcRewardsEvent>(QuestDeliver_CalcRewards);
            SubscribeComponent<QuestTypeDeliverComponent, QuestBeforeGenerateEvent>(QuestDeliver_BeforeGenerate);
            SubscribeComponent<QuestTypeDeliverComponent, QuestBeforeAcceptEvent>(QuestDeliver_BeforeAccept);
            SubscribeComponent<QuestTypeDeliverComponent, QuestGetTargetCharasEvent>(QuestDeliver_GetTargetCharas);
            SubscribeComponent<QuestTypeDeliverComponent, QuestFailedEvent>(QuestDeliver_OnFailed);

            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestDeliver_AddGiveDialogChoices);
        }

        private void QuestDeliver_Localize(EntityUid uid, QuestTypeDeliverComponent questDeliver, QuestLocalizeDataEvent args)
        {
            args.OutParams["itemName"] = _quests.FormatQuestObjective(_itemName.QualifyNameWithItemType(questDeliver.TargetItemID));
            args.OutParams["itemCategory"] = Loc.GetPrototypeString(questDeliver.TargetItemCategory, "Name");
            args.OutParams["targetCharaName"] = questDeliver.TargetCharaName;
            args.OutParams["targetMapName"] = questDeliver.TargetMapName;
            args.OutDetailLocaleKey = "Elona.Quest.Types.Deliver.Detail";
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

        private record class GenericQuestTarget(IArea DestArea, MapId DestMapID, QuestHubData DestQuestHub, QuestClient DestClient);

        private record class GenericQuestTargetAndDist(GenericQuestTarget Target, int DistanceTiled);

        private bool FindDeliveryTarget(GlobalAreaId sourceAreaID, IList<GenericQuestTarget> candidates, [NotNullWhen(true)] out GenericQuestTargetAndDist? target)
        {
            var knownEntrances = _areaKnownEntrances.EnumerateKnownEntrancesTo(sourceAreaID).ToList();
            _rand.Shuffle(knownEntrances);

            foreach (var cand in candidates)
            {
                foreach (var entrance in knownEntrances)
                {
                    if (cand.DestArea.GlobalId == null || cand.DestArea.GlobalId == sourceAreaID)
                        continue;

                    if (_areaKnownEntrances.TryDistanceTiled(entrance.MapCoordinates, cand.DestMapID, out var distance))
                    {
                        target = new GenericQuestTargetAndDist(cand, distance);
                        return true;
                    }
                }
            }

            target = null;
            return false;
        }

        private IList<GenericQuestTarget> GetDeliverQuestTargets(GlobalAreaId sourceAreaId)
        {
            bool IsAlreadyDeliverQuestTarget(EntityUid client)
            {
                return _quests.EnumerateAllQuests<QuestTypeDeliverComponent>()
                    .Any(pair => pair.QuestType.TargetChara == client);
            }

            bool CanBeDeliverQuestTarget(MapId mapID, EntityUid client)
            {
                if (!TryArea(mapID, out var area) || area.GlobalId == null)
                    return false;

                if (IsAlreadyDeliverQuestTarget(client))
                    return false;

                return true;
            }

            var candidates = _quests.EnumerateQuestHubs()
               .SelectMany(t => t.QuestHub.Clients.Values.Select(c => new GenericQuestTarget(t.Area, t.MapId, t.QuestHub, c)))
               .Where(p => CanBeDeliverQuestTarget(p.DestMapID, p.DestClient.ClientEntityUid))
               .ToList();
            _rand.Shuffle(candidates);
            return candidates;
        }

        private void QuestDeliver_BeforeGenerate(EntityUid uid, QuestTypeDeliverComponent questDeliver, QuestBeforeGenerateEvent args)
        {
            if (!_areaManager.TryGetAreaOfMap(args.Map, out var sourceArea))
            {
                Logger.WarningS("quest.deliver", $"No area in map {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName})");
                args.Cancel();
                return;
            }

            if (sourceArea.GlobalId == null)
            {
                Logger.ErrorS("quest.deliver", $"Area {sourceArea} {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName}) is not global");
                args.Cancel();
                return;
            }

            var candidates = GetDeliverQuestTargets(sourceArea.GlobalId.Value);

            if (!FindDeliveryTarget(sourceArea.GlobalId.Value, candidates, out var targetAndDist))
            {
                Logger.ErrorS("quest.deliver", $"No deliver clients found outside map {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName})");
                args.Cancel();
                return;
            }

            var target = targetAndDist.Target;
            questDeliver.TargetMapID = target.DestMapID;
            questDeliver.TargetMapName = target.DestQuestHub.MapName;
            questDeliver.TargetChara = target.DestClient.ClientEntityUid;
            questDeliver.TargetCharaName = target.DestClient.ClientName;

            Logger.DebugS("quest.deliver", $"FOUND deliver client: {target.DestMapID} ({target.DestQuestHub.MapName}) {target.DestClient.ClientName}");

            questDeliver.TargetItemCategory = _randomGen.PickTag(Protos.TagSet.ItemDeliver);
            var itemID = _itemGen.PickRandomItemIdRaw(args.Map, tags: new[] { questDeliver.TargetItemCategory });
            if (itemID == null)
            {
                Logger.ErrorS("quest.deliver", $"No valid item ID found for category {questDeliver.TargetItemCategory} - {args.Quest.ClientOriginatingMapID} ({args.Quest.ClientOriginatingMapName})");
                args.Cancel();
                return;
            }

            questDeliver.TargetItemID = itemID.Value;
            args.Quest.LocaleKeyRoot = new LocaleKey($"Elona.Quest.Types.Deliver.Categories.{questDeliver.TargetItemCategory}.Variants");

            var rewardGold = EnsureComp<QuestRewardGoldComponent>(uid);
            rewardGold.GoldModifier = 70 + targetAndDist.DistanceTiled * 2;
            if (AreaIsNoyel(sourceArea) || AreaIsNoyel(target.DestArea))
            {
                rewardGold.GoldModifier = (int)(rewardGold.GoldModifier * 1.75f);
            }

            if (_protos.TryGetExtendedData(questDeliver.TargetItemCategory, out ExtDeliveryQuestRewardType? extRewardType))
            {
                if (extRewardType.RandomCategory != null)
                {
                    EnsureComp<QuestRewardRandomCategoryComponent>(uid).ItemCategories = extRewardType.RandomCategory.Value;
                }
                if (extRewardType.SingleCategory != null)
                {
                    EnsureComp<QuestRewardSingleCategoryComponent>(uid).ItemCategory = extRewardType.SingleCategory.Value;
                }
            }
            else
            {
                Logger.ErrorS("quest.deliver", $"No delivery reward found for item type: {questDeliver.TargetItemCategory}");
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

        private void QuestDeliver_GetTargetCharas(EntityUid questUid, QuestTypeDeliverComponent deliverQuest, QuestGetTargetCharasEvent args)
        {
            args.OutTargetCharas.Add(deliverQuest.TargetChara);
        }

        private void QuestDeliver_OnFailed(EntityUid uid, QuestTypeDeliverComponent component, QuestFailedEvent args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:350 		if qExist(rq)=qDeliver{ ...
            _mes.Display(Loc.GetString("Elona.Quest.Types.Deliver.Fail"), color: UiColors.MesPurple);
            _karma.ModifyKarma(_gameSession.Player, -20);
            // <<<<<<<< elona122/shade2/quest.hsp:354 			} ...
        }
    }

    /// <summary>
    /// If an item with this major type is generated as the delivery quest item,
    /// set the type of quest reward to one of these categories.
    /// </summary>
    public sealed class ExtDeliveryQuestRewardType : IPrototypeExtendedData<TagPrototype>
    {
        /// <summary>
        /// If non-null, the reward type will be picked from an item category.
        /// </summary>
        [DataField]
        public PrototypeId<TagPrototype>? SingleCategory { get; set; }

        /// <summary>
        /// If non-null, the reward type will be picked from a set of item categories.
        /// </summary>
        [DataField]
        public PrototypeId<TagSetPrototype>? RandomCategory { get; set; }
    
        // TODO validate Either-ish type
    }
}