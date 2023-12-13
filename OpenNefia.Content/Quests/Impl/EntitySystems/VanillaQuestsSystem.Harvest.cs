using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Log;
using OpenNefia.Content.Damage;
using OpenNefia.Content.World;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Inventory;
using OpenNefia.Core;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        private void Initialize_Harvest()
        {
            SubscribeComponent<QuestTypeHarvestComponent, QuestLocalizeDataEvent>(QuestHarvest_Localize);
            SubscribeComponent<QuestTypeHarvestComponent, QuestCalcDifficultyEvent>(QuestHarvest_CalcDifficulty);
            SubscribeComponent<QuestTypeHarvestComponent, QuestCalcRewardsEvent>(QuestHarvest_CalcRewards);
            SubscribeComponent<QuestTypeHarvestComponent, QuestBeforeGenerateEvent>(QuestHarvest_BeforeGenerate);
            SubscribeComponent<QuestTypeHarvestComponent, QuestBeforeAcceptEvent>(QuestHarvest_BeforeAccept);
            SubscribeComponent<QuestTypeHarvestComponent, QuestTimerExpiredEvent>(QuestHarvest_TimerExpired);

            SubscribeComponent<InventoryComponent, BeforePartyMemberLeavesMapEventArgs>(RemoveQuestItemsInInventory);
            SubscribeComponent<MapImmediateQuestComponent, AfterMapEnterEventArgs>(QuestHarvest_ShowMessage);

            SubscribeComponent<HarvestDeliveryChestComponent, GetVerbsEventArgs>(HarvestDeliveryChest_GetVerbs);
            SubscribeComponent<HarvestQuestCropComponent, GetDisplayNameEventArgs>(HarvestCrop_GetDisplayName);
            SubscribeComponent<HarvestQuestCropComponent, GetVerbsEventArgs>(HarvestCrop_GetVerbs);
            SubscribeComponent<HarvestQuestCropComponent, InventoryContextFilterEvent>(HarvestCrop_InventoryFilter);
        }

        private void QuestHarvest_Localize(EntityUid uid, QuestTypeHarvestComponent questConquer, QuestLocalizeDataEvent args)
        {
            // TODO journal
            var requiredWeight = UiUtils.DisplayWeight(questConquer.RequiredWeight);
            var objective = Loc.GetString("Elona.Quest.Types.Harvest.Detail.Objective", ("requiredWeight", requiredWeight));
            if (_immediateQuests.TryGetImmediateQuest(_mapManager.ActiveMap!, out var quest, out _) && quest.Owner == args.Quest.Owner)
                objective += Loc.Space + Loc.GetString("Elona.Quest.Types.Harvest.Detail.Now", ("currentWeight", UiUtils.DisplayWeight(questConquer.CurrentWeight)));

            args.OutParams["objective"] = objective;
            args.OutParams["requiredWeight"] = requiredWeight;
        }

        private void QuestHarvest_CalcDifficulty(EntityUid uid, QuestTypeHarvestComponent harvestQuest, QuestCalcDifficultyEvent args)
        {
            var playerLevel = _levels.GetLevel(_gameSession.Player);
            var playerFame = _fame.GetFame(_gameSession.Player);
            var difficulty = int.Clamp(_rand.Next(playerLevel + 5) + _rand.Next(playerFame / 800 + 1) + 1, 1, 50);
            args.OutDifficulty = difficulty;
        }

        private bool WasHarvestGreatSuccess(QuestTypeHarvestComponent questHarvest)
        {
            return (questHarvest.RequiredWeight * 1.25) < questHarvest.CurrentWeight;
        }

        private void QuestHarvest_CalcRewards(EntityUid uid, QuestTypeHarvestComponent component, QuestCalcRewardsEvent args)
        {
            // >>>>>>>> shade2/quest.hsp:456 	if qExist(rq)=qHarvest:if qParam1(rq)!0:if qParam ...
            if (WasHarvestGreatSuccess(component))
            {
                args.OutGold = int.Clamp(args.OutGold * (component.CurrentWeight / component.RequiredWeight), args.OutGold, args.OutGold * 3);
            }
            // <<<<<<<< shade2/quest.hsp:456 	if qExist(rq)=qHarvest:if qParam1(rq)!0:if qParam ..
        }

        private void QuestHarvest_BeforeGenerate(EntityUid uid, QuestTypeHarvestComponent harvestQuest, QuestBeforeGenerateEvent args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:216 		rewardFix	=60+qLevel(rq)*2 ...
            EnsureComp<QuestRewardGoldComponent>(args.Quest.Owner).GoldModifier = 60 * args.Quest.Difficulty * 2;
            // <<<<<<<< elona122/shade2/quest.hsp:216 		rewardFix	=60+qLevel(rq)*2 ...

            harvestQuest.RequiredWeight = 15000 + args.Quest.Difficulty * 2500;
            harvestQuest.CurrentWeight = 0;
        }

        private void QuestHarvest_ShowMessage(EntityUid entity, MapImmediateQuestComponent comp, AfterMapEnterEventArgs args)
        {
            if (_immediateQuests.TryGetImmediateQuest<QuestTypeHarvestComponent>(args.NewMap, out _, out _, out var questHarvest))
            {
                // >>>>>>>> elona122/shade2/map_rand.hsp:378 	flt:item_create -1,idChestPay,cX(pc)+1,cY(pc):iPr ...
                var harvestChest = _lookup.EntityQueryInMap<HarvestDeliveryChestComponent>(args.NewMap).FirstOrDefault();
                if (harvestChest == null)
                {
                    var newHarvestChest = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.MastersDeliveryChest);
                    if (IsAlive(newHarvestChest))
                        EnsureComp<PickableComponent>(newHarvestChest.Value).OwnState = OwnState.NPC;
                }
                // <<<<<<<< elona122/shade2/map_rand.hsp:378 	flt:item_create -1,idChestPay,cX(pc)+1,cY(pc):iPr ...

                // >>>>>>>> shade2/map.hsp:2161 		if gQuest=qHarvest{ ...
                if (questHarvest.RequiredWeight <= 0)
                {
                    questHarvest.RequiredWeight = 15000;
                    EnsureComp<QuestRewardGoldComponent>(questHarvest.Owner).GoldModifier = 400;
                }

                var timeRemaining = _immediateQuests.GetImmediateQuestRemainingTime(args.NewMap);
                _mes.Display(Loc.GetString("Elona.Quest.Types.Harvest.Event.OnMapEnter", ("requiredWeight", UiUtils.DisplayWeight(questHarvest.RequiredWeight)), ("timeLimitMinutes", timeRemaining.TotalMinutes)), color: UiColors.MesSkyBlue);
                // <<<<<<<< shade2/map.hsp:2164 			} ..
            }
        }

        private void QuestHarvest_TimerExpired(EntityUid uid, QuestTypeHarvestComponent component, QuestTimerExpiredEvent args)
        {
            // >>>>>>>> shade2/main.hsp:1625 	if gQuest=qHarvest{ ...
            if (component.RequiredWeight <= component.CurrentWeight)
            {
                args.Quest.State = QuestState.Completed;
                _mes.Display(Loc.GetString("Elona.Quest.Types.Harvest.Event.Complete"), color: UiColors.MesGreen);
                _playerQuery.PromptMore();
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Quest.Types.Harvest.Event.Fail"), color: UiColors.MesPurple);
            }

            _mapEntrances.UseMapEntrance(_gameSession.Player, args.ImmediateQuest.PreviousLocation);
            // <<<<<<<< shade2/main.hsp:1635 	if gQuest=qConquer{ ..
        }

        private void QuestHarvest_BeforeAccept(EntityUid uid, QuestTypeHarvestComponent harvestQuest, QuestBeforeAcceptEvent args)
        {
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestHarvest, "Accept");
        }

        public QualifiedDialogNode? QuestHarvest_TravelToMap(IDialogEngine engine, IDialogNode node)
        {
            var quest = engine.Data.Get<DialogQuestData>();
            var questConquer = EnsureComp<QuestTypeHarvestComponent>(quest.Quest.Owner);

            var map = GetMap(engine.Speaker!.Value);

            // TODO: It would be nice to override the map generation routine based on the current area, etc.
            var conquerMap = QuestMap_GenerateHarvest(quest.Quest.Difficulty);

            var spatial = Spatial(_gameSession.Player);
            var prevLocation = MapEntrance.FromMapCoordinates(spatial.MapPosition);

            // >>>>>>>> shade2/map_rand.hsp:335 	gTimeLimit=120:gCountNotice=9999 ...
            _immediateQuests.SetImmediateQuest(conquerMap, quest.Quest, prevLocation, timeLimit: GameTimeSpan.FromHours(2));
            // <<<<<<<< shade2/map_rand.hsp:334 *map_createDungeonHarvest ..
            _mapEntrances.SetPreviousMap(conquerMap, spatial.MapPosition);

            _mapTransfer.DoMapTransfer(spatial, conquerMap, new QuestHarvestMapLocation());
            return null;
        }

        private void QuestHarvest_QuestCompleted(EntityUid uid, QuestTypeHarvestComponent component, QuestCompletedEvent args)
        {
            if (args.DialogEngine != null)
                args.DialogEngine.Data.Ensure<DialogQuestHarvestResultData>().WasGreatSuccess = WasHarvestGreatSuccess(component);
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestHarvest, "Complete");
        }

        public QualifiedDialogNode? QuestHarvest_Complete(IDialogEngine engine, IDialogNode node)
        {
            var partyData = engine.Data.Get<DialogQuestHarvestResultData>();

            var text = Loc.GetString("Elona.Quest.Dialog.Complete.DoneWell", ("speaker", engine.Speaker));
            if (partyData.WasGreatSuccess)
            {
                text += Loc.Space + Loc.GetString("Elona.Quest.Types.Harvest.Dialog.GiveExtraCoins", ("speaker", engine.Speaker));
            }

            var texts = new List<DialogTextEntry>()
            {
                DialogTextEntry.FromString(text)
            };
            var nextNodeID = new QualifiedDialogNodeID(Protos.Dialog.Default, "Talk");

            var newNode = new DialogJumpNode(texts, nextNodeID);
            return new QualifiedDialogNode(Protos.Dialog.Guard, newNode);
        }

        public sealed class DialogQuestHarvestResultData : IDialogExtraData
        {
            [DataField]
            public bool WasGreatSuccess { get; set; } = false;
        }
    }

    public sealed class QuestHarvestMapLocation : IMapStartLocation
    {
        [Dependency] private readonly IRandom _rand = default!;

        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            EntitySystem.InjectDependencies(this);
            return _rand.NextVec2iInVec(map.Size / 3) + map.Size / 3;
        }
    }

    public sealed class QuestHarvestCharaFilterGen : IMapCharaFilterGen
    {
        [Dependency] private readonly IMapImmediateQuestSystem _immQuests = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;

        public QuestHarvestCharaFilterGen() { }

        public QuestHarvestCharaFilterGen(int difficulty)
        {
            Difficulty = difficulty;
        }

        [DataField]
        public int? Difficulty { get; set; } = null;

        public CharaFilter GenerateFilter(IMap map)
        {
            // >>>>>>>> shade2/map.hsp:70 	if gArea=areaQuest{ ..
            var questDifficulty = Difficulty;
            if (questDifficulty == null && _immQuests.TryGetImmediateQuest(map, out var quest, out _))
                questDifficulty = quest.Difficulty;

            if (questDifficulty == null)
                Logger.ErrorS("quest.harvest", "No immediate quest found in harvest character generation!");

            return new CharaFilter()
            {
                MinLevel = int.Clamp(_randomGen.CalcObjectLevel((questDifficulty ?? 1)) / 4, 1, 8),
                Quality = _randomGen.CalcObjectQuality(Quality.Normal),
                Tags = new[] { Protos.Tag.CharaWild }
            };
            // <<<<<<<< shade2/map.hsp:76 		} ..
        }
    }
}