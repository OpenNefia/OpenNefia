using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.World;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Maps;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Fame;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Utility;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Effects.New.Unique;
using OpenNefia.Content.Return;

namespace OpenNefia.Content.Quests
{
    public sealed partial class QuestSystem
    {
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IEmotionIconSystem _emotionIcons = default!;

        public override void Initialize()
        {
            SubscribeEntity<AfterAreaFloorGeneratedEvent>(HandleAddQuestHub);

            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestClient_GetDefaultDialogChoices);
            SubscribeComponent<QuestClientComponent, BeforeStepDialogEvent>(BeforeStepDialog_TurnInQuest);
            SubscribeBroadcast<QuestCanGenerateEvent>(FilterQuestsByPlayerFame);

            SubscribeComponent<QuestDeadlinesComponent, QuestBeforeGenerateEvent>(QuestDeadlines_BeforeGenerate, priority: EventPriorities.VeryHigh);

            SubscribeComponent<QuestComponent, QuestCompletedEvent>(Quest_Completed, priority: EventPriorities.VeryHigh);
            SubscribeComponent<QuestComponent, QuestGenerateRewardsEvent>(Quest_GenerateRewards, priority: EventPriorities.VeryHigh);
            SubscribeComponent<QuestRewardGoldComponent, QuestCalcRewardsEvent>(QuestRewardGold_CalcRewards, priority: EventPriorities.Highest);
            SubscribeComponent<QuestRewardRandomCategoryComponent, QuestGenerateRewardsEvent>(QuestRewardRandomCategory_GenerateRewards);
            SubscribeComponent<QuestRewardSingleCategoryComponent, QuestGenerateRewardsEvent>(QuestRewardSingleCategory_GenerateRewards);

            SubscribeComponent<QuestRewardGoldComponent, QuestLocalizeRewardsEvent>(QuestRewardGold_LocalizeRewards, priority: EventPriorities.Highest);
            SubscribeComponent<QuestRewardRandomCategoryComponent, QuestLocalizeRewardsEvent>(QuestRewardRandomCategory_LocalizeRewards);
            SubscribeComponent<QuestRewardSingleCategoryComponent, QuestLocalizeRewardsEvent>(QuestRewardSingleCategory_LocalizeRewards);

            SubscribeBroadcast<QuestBeforeAcceptEvent>(QuestBeforeAccept_UpdateQuestTargets);
            SubscribeBroadcast<AfterMapEnterEventArgs>(AfterMapEnter_UpdateQuestTargets);
            SubscribeBroadcast<MapOnTimePassedEvent>(TimePassed_CheckQuestDeadlines);
            SubscribeEntity<MapBeforeTurnBeginEventArgs>(BeforeTurnBegin_SetQuestEmoicons);
            SubscribeBroadcast<BeforePlayerCastsReturnMagicEvent>(BeforeCastReturn_CheckFailableQuests);
            SubscribeBroadcast<BeforeReturnMagicExecutedEvent>(CheckAndFailQuests, priority: EventPriorities.Low);
        }

        private void BeforeStepDialog_TurnInQuest(EntityUid uid, QuestClientComponent component, BeforeStepDialogEvent args)
        {
            // >>>>>>>> elona122/shade2/chat.hsp:2315 	if gLevel=1{ ...
            var completedQuest = EnumerateAllQuestsForClient(uid).FirstOrDefault(q => q.State == QuestState.Completed);
            if (completedQuest != null)
            {
                var nextNodeId = TurnInQuest(completedQuest.Owner, uid, args.DialogEngine);
                args.OutCurrentNode = args.DialogEngine.GetNodeByID(nextNodeId).Node;
            }
            // <<<<<<<< elona122/shade2/chat.hsp:2356 		} ...
        }

        private void QuestBeforeAccept_UpdateQuestTargets(QuestBeforeAcceptEvent ev)
        {
            UpdateQuestTargets();
        }

        private void AfterMapEnter_UpdateQuestTargets(AfterMapEnterEventArgs ev)
        {
            UpdateQuestTargets();
        }

        private void TimePassed_CheckQuestDeadlines(ref MapOnTimePassedEvent ev)
        {
            // >>>>>>>> elona122/shade2/main.hsp:682 		evAdd evQuestCheck ...
            if (ev.DaysPassed <= 0)
                return;
            // <<<<<<<< elona122/shade2/main.hsp:682 		evAdd evQuestCheck ...

            // >>>>>>>> elona122/shade2/quest.hsp:298 *quest_check ...
            foreach (var quest in EnumerateAcceptedQuests().ToList())
            {
                if (quest.Deadline != null && _world.State.GameDate >= quest.Deadline.Value)
                {
                    FailQuest(quest.Owner);
                }
            }
            // <<<<<<<< elona122/shade2/quest.hsp:311 	return ...
        }

        public void UpdateQuestTargets()
        {
            foreach (var quest in EnumerateAcceptedOrCompletedQuests())
            {
                quest.TargetEntities.Clear();
                var ev = new QuestGetTargetCharasEvent(quest);
                RaiseEvent(quest.Owner, ev);
                quest.TargetEntities.AddRange(ev.OutTargetCharas);
            }
        }

        private void BeforeTurnBegin_SetQuestEmoicons(EntityUid uid, MapBeforeTurnBeginEventArgs args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/calculation.hsp:1284 		if cQuestNpc(r1)!0{ ...
            if (!TryMap(uid, out var map))
                return;

            var quests = EnumerateAcceptedOrCompletedQuests().ToList();
            foreach (var quest in quests)
            {
                foreach (var entity in quest.TargetEntities)
                {
                    if (EntityManager.EntityExists(entity) && Spatial(entity).MapID == map.Id)
                    {
                        _emotionIcons.SetEmotionIcon(entity, EmotionIcons.QuestTarget);
                    }
                }

                if (EntityManager.EntityExists(quest.ClientEntity) && Spatial(quest.ClientEntity).MapID == map.Id)
                {
                    if (quest.State == QuestState.Completed)
                    {
                        _emotionIcons.SetEmotionIcon(quest.ClientEntity, EmotionIcons.QuestClient);
                    }
                }
            }

            // <<<<<<<< elona122/shade2/calculation.hsp:1288 			} ...
        }

        public bool IsReturnForbiddenByActiveQuest()
        {
            return EnumerateAcceptedQuests()
                .Any(q =>
                {
                    if (!TryComp<QuestFailureConditionsComponent>(q.Owner, out var qCond))
                        return false;

                    return qCond.IsReturnForbidden;
                });
        }

        private void BeforeCastReturn_CheckFailableQuests(BeforePlayerCastsReturnMagicEvent ev)
        {
            // >>>>>>>> elona122/shade2/command.hsp:4372 *check_return ...
            if (IsReturnForbiddenByActiveQuest())
            {
                ev.OutWarningReasons.Add(new(Loc.GetString("Elona.Quest.ReturnIsForbidden"), PreventReturnSeverity.PromptYesNo));
            }
            // <<<<<<<< elona122/shade2/command.hsp:4386 	p=0 ...
        }

        /// <summary>
        /// NOTE: For now has only a karma penalty.
        /// </summary>
        private void CheckAndFailQuests(BeforeReturnMagicExecutedEvent args)
        {
            if (args.Cancelled)
                return;

            // >>>>>>>> elona122/shade2/main.hsp:752 				gosub *check_return :if stat=true:txtMore:txt  ...
            if (IsReturnForbiddenByActiveQuest())
            {
                _mes.Display(Loc.GetString("Elona.Return.Result.CommitCrime"));
                _karma.ModifyKarma(_gameSession.Player, -10);
            }
            // <<<<<<<< elona122/shade2/main.hsp:752 				gosub *check_return :if stat=true:txtMore:txt  ...
        }

        private void HandleAddQuestHub(EntityUid areaEntityUid, AfterAreaFloorGeneratedEvent ev)
        {
            UpdateQuestHubRegistration(ev.Map, ev.Area);
        }

        private void QuestClient_GetDefaultDialogChoices(EntityUid uid, QuestClientComponent component, GetDefaultDialogChoicesEvent args)
        {
            // TODO multiple quests per client
            foreach (var quest in EnumerateAllQuestsForClient(uid).Where(q => q.State == QuestState.NotAccepted))
            {
                args.OutChoices.Add(new DialogChoiceEntry()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Quest.Dialog.Choices.About"),
                    NextNode = new(Protos.Dialog.QuestClient, "About"),
                    ExtraData = new List<IDialogExtraData>()
                    {
                        new DialogQuestData(quest)
                    }
                });
            }
        }

        private void QuestDeadlines_BeforeGenerate(EntityUid uid, QuestDeadlinesComponent questDeadlines, QuestBeforeGenerateEvent args)
        {
            if (questDeadlines.TownBoardExpirationDays != null)
                args.Quest.TownBoardExpirationDate = _world.State.GameDate + GameTimeSpan.FromDays(_rand.NextIntInRange(questDeadlines.TownBoardExpirationDays.Value));

            if (questDeadlines.DeadlineDays != null)
                args.Quest.TimeAllotted = GameTimeSpan.FromDays(_rand.NextIntInRange(questDeadlines.DeadlineDays.Value));
        }

        private void Quest_Completed(EntityUid uid, QuestComponent component, QuestCompletedEvent args)
        {
            CreateQuestRewards(component);

            var player = _gameSession.Player;
            _karma.ModifyKarma(player, 1);

            _mes.Display(Loc.GetString("Elona.Quest.CompletedTakenFrom", ("clientName", component.ClientName)), UiColors.MesGreen);

            if (TryComp<FameComponent>(_gameSession.Player, out var fame))
            {
                var baseFameGained = component.Difficulty * 3 + 10;
                var fameGained = _fame.CalcFameGained(player, baseFameGained);
                fame.Fame.Base += fameGained;
                _mes.Display(Loc.GetString("Elona.Fame.Gain", ("fameGained", fameGained)), UiColors.MesGreen);
            }

            _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
            _audio.Play(Protos.Sound.Complete1);
        }

        private int CalcDefaultRewardItemCount()
        {
            return _rand.Next(_rand.Next(4) + 1) + 1;
        }

        private void CreateQuestRewards(QuestComponent quest)
        {
            var ev = new QuestCalcRewardsEvent(quest, 0, 1, CalcDefaultRewardItemCount());
            RaiseEvent(quest.Owner, ev);

            var ev2 = new QuestGenerateRewardsEvent(quest, ev.OutGold, ev.OutPlatinum, ev.OutItemCount);
            RaiseEvent(quest.Owner, ev2);
        }

        private void Quest_GenerateRewards(EntityUid uid, QuestComponent component, QuestGenerateRewardsEvent ev)
        {
            var player = _gameSession.Player;

            if (ev.Gold > 0)
                _itemGen.GenerateItem(player, Protos.Item.GoldPiece, amount: ev.Gold);
            if (ev.Platinum > 0)
                _itemGen.GenerateItem(player, Protos.Item.PlatinumCoin, amount: ev.Platinum);
        }

        private int CalcQuestRewardGoldAmount(EntityUid uid, QuestRewardGoldComponent component, QuestComponent quest)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:34 	qReward(rq)	=((qLevel(rq)+3)*100+rnd(qLevel(rq)*3 ...
            int amount = _rand.WithSeed(quest.RandomSeed, () =>
            {
                var amount = ((quest.Difficulty + 3) * 100 + _rand.Next(quest.Difficulty * 30 + 200) + 400) * component.GoldModifier / 100;
                amount = amount * 100 / (100 + quest.Difficulty * 2 / 3);
                return amount;
            });

            if (!component.ModifyGoldBasedOnPlayerLevel)
                return amount;
            // <<<<<<<< elona122/shade2/quest.hsp:36 	if (qType(rq)=qTypeSupply)or(qType(rq)=qTypeDeliv ...

            return CalcQuestRewardGoldAmountFromPlayerLevel(uid, component, quest, amount);
        }

        private int CalcQuestRewardGoldAmountFromPlayerLevel(EntityUid uid, QuestRewardGoldComponent component, QuestComponent quest, int baseAmount)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:37 	if cLevel(pc)>=qLevel(rq){ ...
            var level = _levels.GetLevel(_gameSession.Player);
            if (level >= quest.Difficulty)
            {
                return baseAmount * 10 / (100 + (level - quest.Difficulty) * 10);
            }
            else
            {
                return baseAmount * (100 + Math.Clamp((quest.Difficulty - level) / 5 * 25, 0, 200)) / 100;
            }
            // <<<<<<<< elona122/shade2/quest.hsp:41 		} ...
        }

        private void QuestRewardGold_CalcRewards(EntityUid uid, QuestRewardGoldComponent component, QuestCalcRewardsEvent args)
        {
            args.OutGold = CalcQuestRewardGoldAmount(uid, component, args.Quest);
        }

        private void QuestRewardRandomCategory_GenerateRewards(EntityUid uid, QuestRewardRandomCategoryComponent component, QuestGenerateRewardsEvent args)
        {
            for (var i = 0; i < args.ItemCount; i++)
            {
                var tag = _randomGen.PickTag(component.ItemCategories);
                ItemFilter filter = GetQuestRewardItemFilter(args.Quest, tag);
                _itemGen.GenerateItem(_gameSession.Player, filter);
            }
        }

        private void QuestRewardSingleCategory_GenerateRewards(EntityUid uid, QuestRewardSingleCategoryComponent component, QuestGenerateRewardsEvent args)
        {
            for (var i = 0; i < args.ItemCount; i++)
            {
                ItemFilter filter = GetQuestRewardItemFilter(args.Quest, component.ItemCategory);
                _itemGen.GenerateItem(_gameSession.Player, filter);
            }
        }

        private void QuestRewardGold_LocalizeRewards(EntityUid uid, QuestRewardGoldComponent component, QuestLocalizeRewardsEvent args)
        {
            var goldAmount = CalcQuestRewardGoldAmount(uid, component, args.Quest);
            args.OutRewardNames.Add(Loc.GetString("Elona.Quest.Rewards.GoldPieces", ("amount", goldAmount)));
        }

        private void QuestRewardRandomCategory_LocalizeRewards(EntityUid uid, QuestRewardRandomCategoryComponent component, QuestLocalizeRewardsEvent args)
        {
            args.OutRewardNames.Add(Loc.GetPrototypeString(component.ItemCategories, "Name"));
        }

        private void QuestRewardSingleCategory_LocalizeRewards(EntityUid uid, QuestRewardSingleCategoryComponent component, QuestLocalizeRewardsEvent args)
        {
            args.OutRewardNames.Add(Loc.GetPrototypeString(component.ItemCategory, "Name"));
        }

        private ItemFilter GetQuestRewardItemFilter(QuestComponent quest, PrototypeId<TagPrototype> itemCategory)
        {
            var quality = Quality.Normal;

            if (_rand.OneIn(2))
            {
                quality = Quality.Good;

                if (_rand.OneIn(12))
                {
                    quality = Quality.Great;
                }
            }

            var filter = new ItemFilter()
            {
                MinLevel = quest.Difficulty + _levels.GetLevel(_gameSession.Player) / 2 + 1,
                Quality = _randomGen.CalcObjectQuality(quality),
                Tags = new[] { itemCategory }
            };

            return filter;
        }
    }
}
