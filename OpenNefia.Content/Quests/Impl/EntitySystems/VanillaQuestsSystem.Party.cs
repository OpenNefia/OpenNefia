using OpenNefia.Content.Dialog;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
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
using OpenNefia.Content.Skills;
using OpenNefia.Content.Talk;
using OpenNefia.Content.Quests;
using OpenNefia.Content.World;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Currency;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.InUse;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.EmotionIcon;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IInUseSystem _inUses = default!;
        [Dependency] private readonly IEmotionIconSystem _emotionIcons = default!;


        private void Initialize_Party()
        {
            SubscribeComponent<QuestTypePartyComponent, QuestLocalizeDataEvent>(QuestParty_Localize);
            SubscribeComponent<QuestTypePartyComponent, QuestCalcDifficultyEvent>(QuestParty_CalcDifficulty);
            SubscribeComponent<QuestTypePartyComponent, QuestBeforeGenerateEvent>(QuestParty_BeforeGenerate);
            SubscribeComponent<QuestTypePartyComponent, QuestBeforeAcceptEvent>(QuestParty_BeforeAccept);
            SubscribeComponent<QuestTypePartyComponent, QuestTimerExpiredEvent>(QuestParty_TimerExpired);
            SubscribeComponent<QuestTypePartyComponent, QuestCompletedEvent>(QuestParty_QuestCompleted);
            SubscribeComponent<QuestTypePartyComponent, QuestGenerateRewardsEvent>(QuestParty_GenerateRewards);

            SubscribeBroadcast<AfterMapEnterEventArgs>(QuestParty_ShowMessage);
            SubscribeEntity<OnAICalmActionEvent>(QuestParty_DrinkingAI);
            SubscribeEntity<MapBeforeTurnBeginEventArgs>(BeforeTurnBegin_SetPartyQuestEmoicons);
        }

        private void QuestParty_Localize(EntityUid uid, QuestTypePartyComponent partyQuest, QuestLocalizeDataEvent args)
        {
            args.OutParams["requiredPoints"] = Loc.GetString("Elona.Quest.Types.Party.Points", ("points", partyQuest.RequiredPoints));
        }

        private void QuestParty_CalcDifficulty(EntityUid uid, QuestTypePartyComponent partyQuest, QuestCalcDifficultyEvent args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:192 		qLevel(rq)	=limit(rnd(sPerform(pc)+10),int(1.5*s ...
            var performerLevel = _skills.Level(_gameSession.Player, Protos.Skill.Performer);
            var playerFame = _fame.GetFame(_gameSession.Player);
            var lowerBound = (int)(1.5 * Math.Sqrt(performerLevel)) + 1;
            var upperBound = (playerFame / 1000) + 10;
            args.OutDifficulty = Math.Clamp(_rand.Next(performerLevel + 10), Math.Min(lowerBound, upperBound), upperBound);
            // <<<<<<<< elona122/shade2/quest.hsp:192 		qLevel(rq)	=limit(rnd(sPerform(pc)+10),int(1.5*s ...
        }

        private void QuestParty_BeforeGenerate(EntityUid uid, QuestTypePartyComponent partyQuest, QuestBeforeGenerateEvent args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:200 		qParam1(rq)	=qLevel(rq)*10+rnd(50) ...
            partyQuest.RequiredPoints = args.Quest.Difficulty * 10 + _rand.Next(50);
            partyQuest.CurrentPoints = 0;
            // <<<<<<<< elona122/shade2/quest.hsp:201 		qParam2(rq)	=0 ...
        }

        private void QuestParty_BeforeAccept(EntityUid uid, QuestTypePartyComponent partyQuest, QuestBeforeAcceptEvent args)
        {
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestParty, "Accept");
        }

        public void QuestParty_TravelToMap(IDialogEngine engine, IDialogNode node)
        {
            var quest = engine.Data.Get<DialogQuestData>();

            // TODO: It would be nice to override the map generation routine based on the current area, etc.
            var partyMap = QuestMap_GenerateParty(quest.Quest.Difficulty);

            var spatial = Spatial(_gameSession.Player);
            var prevLocation = MapEntrance.FromMapCoordinates(spatial.MapPosition);

            _immediateQuests.SetImmediateQuest(partyMap, quest.Quest, GameTimeSpan.FromMinutes(60), prevLocation);

            _mapTransfer.DoMapTransfer(spatial, partyMap, new CenterMapLocation());
        }

        private void QuestParty_ShowMessage(AfterMapEnterEventArgs args)
        {
            // >>>>>>>> shade2/map.hsp:2158 		if gQuest=qPerform{ ...
            if (_immediateQuests.TryGetImmediateQuest<QuestTypePartyComponent>(args.NewMap, out _, out _, out var questParty))
            {
                var timeRemaining = _immediateQuests.GetImmediateQuestRemainingTime(args.NewMap);
                _mes.Display(Loc.GetString("Elona.Quest.Types.Party.Event.OnMapEnter", ("minutes", timeRemaining.TotalMinutes), ("points", questParty.RequiredPoints)), color: UiColors.MesSkyBlue);
            }
            // <<<<<<<< shade2/map.hsp:2160 			} ..
        }

        public int CalcPartyScore(IMap map)
        {
            // >>>>>>>> shade2/calculation.hsp:1339 #deffunc calcPartyScore ..
            var score = 0;
            foreach (var chara in _charas.EnumerateNonAllies(map))
            {
                if (TryComp<DialogComponent>(chara.Owner, out var dialog))
                {
                    if (dialog.Impression >= ImpressionLevels.Party)
                    {
                        score += _levels.GetLevel(chara.Owner) + 5;
                    }
                    if (dialog.Impression < ImpressionLevels.Normal)
                    {
                        score -= 20;
                    }
                }
            }
            return score;
            // <<<<<<<< shade2/calculation.hsp:1349 	return  ..
        }

        public int CalcPartyScoreBonus(IMap map, bool silent = false)
        {
            // >>>>>>>> shade2/calculation.hsp:1351 #deffunc calcPartyScore2 ..
            var bonus = 0;
            foreach (var chara in _charas.EnumerateNonAllies(map))
            {
                if (TryComp<DialogComponent>(chara.Owner, out var dialog))
                {
                    if (dialog.Impression >= ImpressionLevels.Party && _qualities.GetQuality(chara.Owner) >= Quality.Great)
                    {
                        bonus += 20 + _levels.GetLevel(chara.Owner) / 2;
                        if (!silent)
                            _mes.Display(Loc.GetString("Elona.Quest.Types.Party.Event.IsSatisfied", ("entity", chara.Owner)));
                    }
                }
            }
            return bonus;
            // <<<<<<<< shade2/calculation.hsp:1359 	return  .
        }

        public bool WasPartyGreatSuccess(QuestTypePartyComponent component)
        {
            return component.CurrentPoints > component.RequiredPoints * 1.5;
        }

        private void QuestParty_TimerExpired(EntityUid uid, QuestTypePartyComponent component, QuestTimerExpiredEvent args)
        {
            // >>>>>>>> shade2/main.hsp:1609 	if gQuest=qPerform{ ..
            _mes.Display(Loc.GetString("Elona.Quest.Types.Party.Event.IsOver"));

            var score = CalcPartyScore(args.Map);
            var bonus = CalcPartyScoreBonus(args.Map);
            if (bonus > 0)
                _mes.Display(Loc.GetString("Elona.Quest.Types.Party.Event.TotalBonus", ("bonus", bonus)));
            score = (int)(score * ((100 + bonus) / 100.0));

            var silent = false;
            _mes.Display(Loc.GetString("Elona.Quest.Types.Party.Event.FinalScore", ("score", score)));
            if (score >= component.RequiredPoints)
            {
                args.Quest.State = QuestState.Completed;
                _mes.Display(Loc.GetString("Elona.Quest.Types.Party.Event.Complete"), color: UiColors.MesGreen);
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Quest.Types.Party.Event.Fail"), color: UiColors.MesPurple);
                silent = true;
                _quests.FailQuest(args.Quest.Owner);
                _audio.Play(Protos.Sound.Exitmap1);
            }

            _playerQuery.PromptMore();
            _mapEntrances.UseMapEntrance(_gameSession.Player, args.ImmediateQuest.PreviousLocation, silent: silent);
            // <<<<<<<< shade2 / main.hsp:1623      } ..
        }

        private void QuestParty_GenerateRewards(EntityUid uid, QuestTypePartyComponent component, QuestGenerateRewardsEvent args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:463 	if qExist(rq)=qPerform:if qParam1(rq)*150/100<qPa ...
            if (WasPartyGreatSuccess(component))
            {
                var amount = component.CurrentPoints / 10;
                _itemGen.GenerateItem(_gameSession.Player, Protos.Item.MusicTicket, amount: amount);
            }
            // <<<<<<<< elona122/shade2/quest.hsp:463 	if qExist(rq)=qPerform:if qParam1(rq)*150/100<qPa ...
        }

        private void BeforeTurnBegin_SetPartyQuestEmoicons(EntityUid uid, MapBeforeTurnBeginEventArgs args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/calculation.hsp:1291 	if gQuest=qPerform{ ...
            var map = GetMap(uid);
            if (!_immediateQuests.HasImmediateQuest<QuestTypePartyComponent>(map))
                return;

            foreach (var chara in _charas.EnumerateNonAllies(map))
            {
                if (TryComp<DialogComponent>(chara.Owner, out var dialog) && dialog.Impression >= ImpressionLevels.Party)
                    _emotionIcons.SetEmotionIcon(chara.Owner, EmotionIcons.Party);
            }
            // <<<<<<<< elona122/shade2/calculation.hsp:1293 		} ...
        }

        private void QuestParty_DrinkingAI(EntityUid uid, OnAICalmActionEvent args)
        {
            // >>>>>>>> elona122/shade2/ai.hsp:232 	if cAiItem(cc)=0:if cRelation(cc)!cAlly{ ...
            if (args.Handled)
                return;

            var map = GetMap(uid);
            if (!_immediateQuests.HasImmediateQuest<QuestTypePartyComponent>(map) || !TryComp<VanillaAIComponent>(uid, out var vanillaAI))
                return;

            if (_inUses.IsUsingAnything(uid) || IsAlive(vanillaAI.ItemAboutToUse) || _factions.GetRelationToPlayer(uid) >= Factions.Relation.Ally)
                return;

            if (_rand.OneIn(30) && TryComp<InventoryComponent>(uid, out var inv))
            {
                PrototypeId<TagPrototype> category;
                if (_rand.OneIn(4))
                    category = Protos.Tag.ItemCatFood;
                else
                    category = Protos.Tag.ItemCatDrink;
                if (!_rand.OneIn(8))
                    category = Protos.Tag.ItemCatDrinkAlcohol;

                var filter = new ItemFilter()
                {
                    Tags  = new[] { category },
                    MinLevel = 20,
                };  
                var item = _itemGen.GenerateItem(inv.Container, filter);
                if (IsAlive(item))
                    vanillaAI.ItemAboutToUse = item;
            }
            // <<<<<<<< elona122/shade2/ai.hsp:239 			} ...
        }

        // >>>>>>>> elona122/shade2/text.hsp:1253 		if qExist(rq)=qPerform:if qParam1(rq)*150/100<qP ...
        private void QuestParty_QuestCompleted(EntityUid uid, QuestTypePartyComponent component, QuestCompletedEvent args)
        {
            if (args.DialogEngine != null)
                args.DialogEngine.Data.Ensure<DialogQuestPartyResultData>().WasGreatSuccess = WasPartyGreatSuccess(component);
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestParty, "Complete");
        }

        public QualifiedDialogNode? QuestParty_Complete(IDialogEngine engine, IDialogNode node)
        {
            var partyData = engine.Data.Get<DialogQuestPartyResultData>();

            var text = Loc.GetString("Elona.Quest.Dialog.Complete.DoneWell", ("speaker", engine.Speaker));
            if (partyData.WasGreatSuccess)
            {
                text += Loc.Space + Loc.GetString("Elona.Quest.Types.Party.Dialog.GiveMusicTickets", ("speaker", engine.Speaker));
            }

            var texts = new List<DialogTextEntry>()
            {
                DialogTextEntry.FromString(text)
            };
            var nextNodeID = new QualifiedDialogNodeID(Protos.Dialog.Default, "Talk");

            var newNode = new DialogJumpNode(texts, nextNodeID);
            return new QualifiedDialogNode(Protos.Dialog.Guard, newNode);
        }
        // <<<<<<<< elona122/shade2/text.hsp:1253 		if qExist(rq)=qPerform:if qParam1(rq)*150/100<qP ...

        public sealed class DialogQuestPartyResultData : IDialogExtraData
        {
            [DataField]
            public bool WasGreatSuccess { get; set; } = false;
        }
    }
}