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

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        private void Initialize_HuntEX()
        {
            SubscribeComponent<QuestTypeHuntEXComponent, QuestLocalizeDataEvent>(QuestHuntEX_Localize);
            SubscribeComponent<QuestTypeHuntEXComponent, QuestCalcDifficultyEvent>(QuestHuntEX_CalcDifficulty);
            SubscribeComponent<QuestTypeHuntEXComponent, QuestCalcRewardsEvent>(QuestHuntEX_CalcRewards);
            SubscribeComponent<QuestTypeHuntEXComponent, QuestBeforeGenerateEvent>(QuestHuntEX_BeforeGenerate);
            SubscribeComponent<QuestTypeHuntEXComponent, QuestBeforeAcceptEvent>(QuestHuntEX_BeforeAccept);
            SubscribeComponent<MapImmediateQuestComponent, MapQuestTargetsEliminatedEvent>(QuestHuntEX_TargetsEliminated);
        }

        private void QuestHuntEX_Localize(EntityUid uid, QuestTypeHuntEXComponent questHunt, QuestLocalizeDataEvent args)
        {
            args.OutParams["enemyName"] = Loc.GetPrototypeString(questHunt.EnemyID, "MetaData.Name"); 
            args.OutParams["enemyLevel"] = questHunt.EnemyLevel;
            args.OutDetailLocaleKey = "Elona.Quest.Types.HuntEX.Detail";
        }

        private void QuestHuntEX_CalcDifficulty(EntityUid uid, QuestTypeHuntEXComponent huntQuest, QuestCalcDifficultyEvent args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:111 		qLevel(rq)	=rnd(cLevel(pc)+10)+rnd(cFame(pc)/250 ...
            var playerLevel = _levels.GetLevel(_gameSession.Player);
            var playerFame = _fame.GetFame(_gameSession.Player);
            var difficulty = _rand.Next(playerLevel + 10) + _rand.Next(playerFame / 2500) + 1;
            args.OutDifficulty = _quests.RoundDifficultyMargin(difficulty, playerLevel);
            // <<<<<<<< elona122/shade2/quest.hsp:112 		qLevel(rq)	=roundMargin(qLevel(rq),cLevel(pc))  ...
        }

        private void QuestHuntEX_CalcRewards(EntityUid uid, QuestTypeHuntEXComponent component, QuestCalcRewardsEvent args)
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

        private void QuestHuntEX_BeforeGenerate(EntityUid uid, QuestTypeHuntEXComponent huntQuest, QuestBeforeGenerateEvent args)
        {
            var minLevel = int.Clamp(args.Quest.Difficulty / 7, 5, 30);
            var enemyId = QuestHunt_PickRandomEnemyID(args.Quest, minLevel);

            if (enemyId == null)
            {
                Logger.ErrorS("quest.huntex", $"Failed to pick enemy ID for huntEX quest {uid}!");
                args.Cancel();
                return;
            }

            huntQuest.EnemyID = enemyId.Value;
            huntQuest.EnemyLevel = (int)(minLevel * 1.5);
        }

        private PrototypeId<EntityPrototype>? QuestHunt_PickRandomEnemyID(QuestComponent quest, int minLevel)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:115 		repeat 50 ...
            for (var i = 0; i < 100; i++)
            {
                var commonArgs = new EntityGenCommonArgs()
                {
                    NoRandomModify = true,
                    NoLevelScaling = true,
                    MinLevel = quest.Difficulty,
                    Quality = Qualities.Quality.Normal
                };
                var args = EntityGenArgSet.Make();
                var enemyId = _charaGen.PickRandomCharaId(null, args, minLevel: quest.Difficulty);
                
                if (!_protos.TryIndex(enemyId, out var proto)
                    || !proto.Components.TryGetComponent<LevelComponent>(out var level)
                    || level.Level < minLevel)
                    continue;

                return enemyId;
            }
            return null;
            // <<<<<<<< elona122/shade2/quest.hsp:120 		loop ...
        }

        private void QuestHuntEX_BeforeAccept(EntityUid uid, QuestTypeHuntEXComponent huntQuest, QuestBeforeAcceptEvent args)
        {
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestHuntEX, "Accept");
        }

        public void QuestHuntEX_TravelToMap(IDialogEngine engine, IDialogNode node)
        {
            var quest = engine.Data.Get<DialogQuestData>();
            var questHunt = EnsureComp<QuestTypeHuntEXComponent>(quest.Quest.Owner);

            var map = GetMap(engine.Speaker!.Value);

            // TODO: It would be nice to override the map generation routine based on the current area, etc.
            var partyMap = QuestMap_GenerateHuntEX(map, questHunt.EnemyID, questHunt.EnemyLevel, quest.Quest.Difficulty);

            var spatial = Spatial(_gameSession.Player);
            var prevLocation = MapEntrance.FromMapCoordinates(spatial.MapPosition);

            _immediateQuests.SetImmediateQuest(partyMap, quest.Quest, prevLocation);
            _mapEntrances.SetPreviousMap(partyMap, spatial.MapPosition);

            _mapTransfer.DoMapTransfer(spatial, partyMap, new CenterMapLocation());
        }

        public const string QuestHuntEXTargetTagId = "Elona.QuestHuntEX";

        private void QuestHuntEX_TargetsEliminated(EntityUid uid, MapImmediateQuestComponent component, MapQuestTargetsEliminatedEvent args)
        {
            CheckHuntQuest<QuestTypeHuntEXComponent>(uid, args.Tag, QuestHuntEXTargetTagId);
        }
    }
}