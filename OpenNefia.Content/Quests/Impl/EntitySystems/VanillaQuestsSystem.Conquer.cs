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

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        private void Initialize_Conquer()
        {
            SubscribeComponent<QuestTypeConquerComponent, QuestLocalizeDataEvent>(QuestConquer_Localize);
            SubscribeComponent<QuestTypeConquerComponent, QuestCalcDifficultyEvent>(QuestConquer_CalcDifficulty);
            SubscribeComponent<QuestTypeConquerComponent, QuestCalcRewardsEvent>(QuestConquer_CalcRewards);
            SubscribeComponent<QuestTypeConquerComponent, QuestBeforeGenerateEvent>(QuestConquer_BeforeGenerate);
            SubscribeComponent<QuestTypeConquerComponent, QuestBeforeAcceptEvent>(QuestConquer_BeforeAccept);
            SubscribeComponent<QuestTypeConquerComponent, QuestTimerExpiredEvent>(QuestConquer_TimerExpired);

            SubscribeComponent<MapImmediateQuestComponent, AfterMapEnterEventArgs>(QuestConquer_ShowMessage);

            SubscribeComponent<ConquerQuestTargetComponent, EntityKilledEvent>(ConquerQuestTarget_Killed);
            SubscribeComponent<ConquerQuestTargetComponent, BeforeEntityDeletedEvent>(ConquerQuestTarget_Vanquished);
        }

        private void QuestConquer_Localize(EntityUid uid, QuestTypeConquerComponent questConquer, QuestLocalizeDataEvent args)
        {
            args.OutParams["enemyName"] = Loc.GetPrototypeString(questConquer.EnemyID, "MetaData.Name"); 
            args.OutParams["enemyLevel"] = questConquer.EnemyLevel;
        }

        private void QuestConquer_CalcDifficulty(EntityUid uid, QuestTypeConquerComponent huntQuest, QuestCalcDifficultyEvent args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:134 		qLevel(rq)	=rnd(cLevel(pc)+10)+rnd(cFame(pc)/250 ...
            var playerLevel = _levels.GetLevel(_gameSession.Player);
            var playerFame = _fame.GetFame(_gameSession.Player);
            var difficulty = _rand.Next(playerLevel + 10) + _rand.Next(playerFame / 2500) + 1;
            args.OutDifficulty = _quests.RoundDifficultyMargin(difficulty, playerLevel);
            // <<<<<<<< elona122/shade2/quest.hsp:135 		qLevel(rq)	=roundMargin(qLevel(rq),cLevel(pc))  ...
        }

        private void QuestConquer_CalcRewards(EntityUid uid, QuestTypeConquerComponent component, QuestCalcRewardsEvent args)
        {
            // >>>>>>>> shade2/quest.hsp:459 	if (qExist(rq)=qConquer)or(qExist(rq)=qHuntEx){ ..
            args.OutPlatinum = 2;
            if (_rand.Next(100) < _rand.Next(_fame.GetFame(_gameSession.Player) / 5000 + 1))
            {
                args.OutPlatinum += 1;
            }
            // <<<<<<<< shade2/quest.hsp:461 		} ..

            // >>>>>>>> shade2/quest.hsp:466 		if (qExist(rq)=qConquer)or(qExist(rq)=qHuntEx):p ..
            args.OutItemCount += 2;
            // <<<<<<<< shade2/quest.hsp:466 		if (qExist(rq)=qConquer)or(qExist(rq)=qHuntEx):p ..
        }

        private void QuestConquer_BeforeGenerate(EntityUid uid, QuestTypeConquerComponent huntQuest, QuestBeforeGenerateEvent args)
        {
            var minLevel = int.Clamp(args.Quest.Difficulty / 4, 5, 30);
            var enemyId = QuestHunt_PickRandomEnemyID(args.Quest, minLevel);

            if (enemyId == null)
            {
                Logger.ErrorS("quest.Conquer", $"Failed to pick enemy ID for conquer quest {uid}!");
                args.Cancel();
                return;
            }

            huntQuest.EnemyID = enemyId.Value;
            huntQuest.EnemyLevel = (int)(minLevel * 1.25);
        }

        private void QuestConquer_ShowMessage(EntityUid entity, MapImmediateQuestComponent comp, AfterMapEnterEventArgs args)
        {
            // >>>>>>>> elona122/shade2/map.hsp:2165 		if gQuest=qConquer{ ...
            if (_immediateQuests.TryGetImmediateQuest<QuestTypeConquerComponent>(args.NewMap, out _, out _, out var questConquer))
            {
                var enemyName = Loc.GetPrototypeString(questConquer.EnemyID, "MetaData.Name");
                var timeRemaining = _immediateQuests.GetImmediateQuestRemainingTime(args.NewMap);
                _mes.Display(Loc.GetString("Elona.Quest.Types.Conquer.Event.OnMapEnter", ("enemyName", enemyName), ("timeLimitMinutes", timeRemaining.TotalMinutes)), color: UiColors.MesSkyBlue);
            }
            // <<<<<<<< elona122/shade2/map.hsp:2167 			} ...
        }

        private void QuestConquer_TimerExpired(EntityUid uid, QuestTypeConquerComponent component, QuestTimerExpiredEvent args)
        {
            if (args.Quest.State == QuestState.Completed)
                return;

            // >>>>>>>> shade2/main.hsp:1635 	if gQuest=qConquer{ ..
            _mes.Display(Loc.GetString("Elona.Quest.Types.Conquer.Event.Fail"), color: UiColors.MesPurple);
            // <<<<<<<< shade2/main.hsp:1637 		} ..

            // Jumping to the previous map will automatically fail the active immediate quest,
            // as per IImmediateQuestSystem.
            _mapEntrances.UseMapEntrance(_gameSession.Player, args.ImmediateQuest.PreviousLocation);
        }

        private void QuestConquer_BeforeAccept(EntityUid uid, QuestTypeConquerComponent huntQuest, QuestBeforeAcceptEvent args)
        {
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestConquer, "Accept");
        }

        public QualifiedDialogNode? QuestConquer_TravelToMap(IDialogEngine engine, IDialogNode node)
        {
            var quest = engine.Data.Get<DialogQuestData>();
            var questConquer = EnsureComp<QuestTypeConquerComponent>(quest.Quest.Owner);

            var map = GetMap(engine.Speaker!.Value);

            // TODO: It would be nice to override the map generation routine based on the current area, etc.
            var conquerMap = QuestMap_GenerateConquer(map, questConquer.EnemyID, questConquer.EnemyLevel, quest.Quest.Difficulty, quest.Quest.Owner);

            var spatial = Spatial(_gameSession.Player);
            var prevLocation = MapEntrance.FromMapCoordinates(spatial.MapPosition);

            _immediateQuests.SetImmediateQuest(conquerMap, quest.Quest, prevLocation, timeLimit: GameTimeSpan.FromHours(12));
            _mapEntrances.SetPreviousMap(conquerMap, spatial.MapPosition);

            _mapTransfer.DoMapTransfer(spatial, conquerMap, new CenterMapLocation());
            return null;
        }

        private void FinishConquerQuest(EntityUid uid, ConquerQuestTargetComponent component)
        {
            // >>>>>>>> shade2/chara_func.hsp:194 		if gQuest=qConquer{ ..
            _deferredEvents.Enqueue(() =>
            {
                if (!IsAlive(component.QuestUid)
                    || !TryComp<QuestTypeConquerComponent>(component.QuestUid, out var questConquer)
                    || !TryComp<QuestComponent>(component.QuestUid, out var quest)
                    || quest.State == QuestState.Completed)
                {
                    Logger.ErrorS("quest.conquer", $"Conquer target {uid} had no valid quest!");
                    return TurnResult.Aborted;
                }

                var map = GetMap(uid);

                _music.Play(Protos.Music.Fanfare, loop: false);
                quest.State = QuestState.Completed;
                if (_immediateQuests.TryGetImmediateQuest(map, out var quest2, out _) && quest.Owner == quest2.Owner)
                {
                    _immediateQuests.RemoveImmediateQuestTimer(map);
                }
                else
                {
                    Logger.ErrorS("quest.conquer", $"Conquer target {uid} had no valid immediate quest!");
                }
                _mes.Display(Loc.GetString("Elona.Quest.Types.Conquer.Event.Complete"), color: UiColors.MesGreen);
                return TurnResult.Aborted;
            });
            // <<<<<<<< shade2/chara_func.hsp:196 			} ..
        }

        private void ConquerQuestTarget_Killed(EntityUid uid, ConquerQuestTargetComponent component, ref EntityKilledEvent args)
        {
            FinishConquerQuest(uid, component);
        }

        private void ConquerQuestTarget_Vanquished(EntityUid uid, ConquerQuestTargetComponent component, ref BeforeEntityDeletedEvent args)
        {
            FinishConquerQuest(uid, component);
        }
    }
}