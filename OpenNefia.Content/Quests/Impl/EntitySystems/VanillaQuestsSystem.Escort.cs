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
using OpenNefia.Core.Utility;
using OpenNefia.Core.Maths;
using OpenNefia.Content.World;
using OpenNefia.Content.Areas;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Maps;
using OpenNefia.Content.DeferredEvents;
using Love;
using OpenNefia.Core;
using OpenNefia.Content.Damage;
using ICSharpCode.Decompiler.IL;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Encounters;
using OpenNefia.Content.Mount;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly IMapPlacement _mapPlacements = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IEncounterSystem _encounters = default!;

        private void Initialize_Escort()
        {
            SubscribeComponent<QuestTypeEscortComponent, QuestLocalizeDataEvent>(QuestEscort_Localize);
            SubscribeComponent<QuestTypeEscortComponent, QuestCalcDifficultyEvent>(QuestEscort_CalcDifficulty);
            SubscribeComponent<QuestTypeEscortComponent, QuestBeforeGenerateEvent>(QuestEscort_BeforeGenerate);
            SubscribeComponent<QuestTypeEscortComponent, QuestBeforeAcceptEvent>(QuestEscort_BeforeAccept);
            SubscribeComponent<QuestTypeEscortComponent, QuestFailedEvent>(QuestEscort_OnFailed);

            SubscribeComponent<EscortedInQuestComponent, AfterPartyMemberEntersMapEventArgs>(QuestEscort_CheckDestinationReached);
            SubscribeComponent<EscortedInQuestComponent, EntityKilledEvent>(QuestEscort_CheckKilled);
            SubscribeComponent<EscortedInQuestComponent, BeforeEntityDeletedEvent>(QuestEscort_CheckKilled);
            SubscribeComponent<EscortedInQuestComponent, BeforeEntityStartsRidingEvent>(QuestEscort_BlockRiding);
            SubscribeComponent<EscortedInQuestComponent, BeforeEntityMountedOntoEvent>(QuestEscort_BlockRiding);
            SubscribeBroadcast<MapCalcRandomEncounterIDEvent>(QuestEscort_GenerateEncounter);
        }

        private void QuestEscort_CheckDestinationReached(EntityUid uid, EscortedInQuestComponent comp, AfterPartyMemberEntersMapEventArgs args)
        {
            // >>>>>>>> shade2/main.hsp:1776 	case evClientReached ..
            if (!IsAlive(comp.QuestUid))
                return;

            if (TryComp<QuestTypeEscortComponent>(comp.QuestUid, out var questEscort) && questEscort.TargetMapID == args.NewMap.Id)
            {
                if (IsAlive(questEscort.EscortingChara) && _parties.IsInPlayerParty(questEscort.EscortingChara))
                {
                    _deferredEvents.Enqueue(() =>
                    {
                        _mes.Display(Loc.GetString("Elona.Quest.Completed"), UiColors.MesGreen);
                        _dialog.StartDialog(_gameSession.Player, questEscort.EscortingChara, new QualifiedDialogNodeID(Protos.Dialog.QuestEscort, "Complete"));
                        _quests.TurnInQuest(comp.QuestUid, _gameSession.Player);
                        EntityManager.DeleteEntity(questEscort.EscortingChara);
                        return TurnResult.Aborted;
                    });
                }
                else
                {
                    _deferredEvents.Enqueue(() =>
                    {
                        _quests.FailQuest(comp.QuestUid);
                        EntityManager.DeleteEntity(questEscort.EscortingChara);
                        return TurnResult.Aborted;
                    });
                }
            }
            // <<<<<<<< shade2/main.hsp:1780 	swbreak ..
        }

        private void QuestEscort_CheckKilled(EntityUid uid, EscortedInQuestComponent component, ref EntityKilledEvent args)
        {
            HandleEscortKilled(uid, component);
        }

        private void QuestEscort_CheckKilled(EntityUid uid, EscortedInQuestComponent component, ref BeforeEntityDeletedEvent args)
        {
            HandleEscortKilled(uid, component);
        }

        private void HandleEscortKilled(EntityUid uid, EscortedInQuestComponent component)
        {
            // >>>>>>>> shade2/chara_func.hsp:1674 		if tc!pc :if tc<maxFollower { ..
            _deferredEvents.Enqueue(() =>
            {
                if (!IsAlive(component.QuestUid))
                    return TurnResult.Aborted;

                _quests.FailQuest(component.QuestUid);
                if (IsAlive(uid))
                {
                    EntityManager.GetComponent<CharaComponent>(uid).Liveness = CharaLivenessState.Dead;
                    _parties.RemoveFromCurrentParty(uid);
                }
                return TurnResult.Aborted;
            });
            // <<<<<<<< shade2/chara_func.hsp:1683 			} ..
        }

        // >>>>>>>> elona122/shade2/proc.hsp:2233 	if (cBit(cBodyguard,tc)=true)or(cBit(cGuardTemp,t ...
        private void QuestEscort_BlockRiding(EntityUid uid, EscortedInQuestComponent component, BeforeEntityStartsRidingEvent args)
        {
            if (args.Cancelled)
                return;

            _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.CannotRideClient", ("rider", uid), ("mount", args.Mount)));
            args.Cancel();
        }

        private void QuestEscort_BlockRiding(EntityUid uid, EscortedInQuestComponent component, BeforeEntityMountedOntoEvent args)
        {
            if (args.Cancelled)
                return;

            _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.CannotRideClient", ("rider", args.Rider), ("mount", uid)));
            args.Cancel();
        }
        // <<<<<<<< elona122/shade2/proc.hsp:2236 		} ...

        private void QuestEscort_Localize(EntityUid uid, QuestTypeEscortComponent escortQuest, QuestLocalizeDataEvent args)
        {
            args.OutParams["targetMapName"] = escortQuest.TargetMapName;
            args.OutDetailLocaleKey = "Elona.Quest.Types.Escort.Detail";
        }

        private void QuestEscort_CalcDifficulty(EntityUid uid, QuestTypeEscortComponent escortQuest, QuestCalcDifficultyEvent args)
        {
            // escort quest parameters do not depend on difficulty; difficulty is set during generation
            args.OutDifficulty = 0;
        }

        private void QuestEscort_BeforeGenerate(EntityUid uid, QuestTypeEscortComponent escortQuest, QuestBeforeGenerateEvent args)
        {
            if (!TryMap(args.Quest.ClientEntity, out var sourceMap) || !TryArea(sourceMap.MapEntityUid, out var sourceArea))
            {
                args.Cancel();
                return;
            }

            var destCandidates = _quests.EnumerateQuestHubs()
               .SelectMany(t => t.QuestHub.Clients.Values.Select(c => new GenericQuestTarget(t.Area, t.MapId, t.QuestHub, c)))
               .Where(p => sourceMap.Id != p.DestMapID)
               .ToList();

            if (destCandidates.Count == 0)
            {
                Logger.ErrorS("quest.escort", $"Failed to generate escort quest: no destination maps found");
                args.Cancel();
                return;
            }

            var dest = _rand.Pick(destCandidates);
            int distance = 0;

            foreach (var entrance in _areaKnownEntrances.EnumerateKnownEntrancesTo(sourceMap))
            {
                if (_areaKnownEntrances.TryDistanceTiled(entrance.MapCoordinates, dest.DestMapID, out distance))
                {
                    break;
                }
            }

            if (distance <= 0)
            {
                Logger.ErrorS("quest.escort", $"Failed to generate escort quest: no destination maps found in range");
                args.Cancel();
                return;
            }

            var playerLevel = _levels.GetLevel(_gameSession.Player);
            var playerFame = _fame.GetFame(_gameSession.Player);
            var quest = args.Quest;
            int rewardGold = 0;

            var escortType = _rand.Pick(EnumHelpers.EnumerateValues<EscortType>().ToList());
            switch (escortType)
            {
                case EscortType.Protect:
                    rewardGold = 140 + distance * 2;
                    quest.TimeAllotted = GameTimeSpan.FromDays(_rand.NextIntInRange(new IntRange(6, 14)));
                    quest.Difficulty = Math.Clamp(_rand.Next(playerLevel + 10) + _rand.Next(playerFame / 500 + 1) + 1, 1, 80);
                    break;
                case EscortType.Poison:
                    rewardGold = 130 + distance * 2;
                    quest.TimeAllotted = GameTimeSpan.FromDays(_rand.NextIntInRange(new IntRange(2, 7)));
                    quest.Difficulty = Math.Clamp(rewardGold / 10 + 1, 1, 40);
                    break;
                case EscortType.Deadline:
                    rewardGold = 80 + distance * 2;
                    quest.TimeAllotted = GameTimeSpan.FromDays(_rand.NextIntInRange(new IntRange(6, 14)));
                    quest.Difficulty = Math.Clamp(rewardGold / 20 + 1, 1, 40);
                    break;
            }
            quest.LocaleKeyRoot = new LocaleKey("Elona.Quest.Types.Escort.Variants").With(escortType.ToString());

            if (AreaIsNoyel(sourceArea) || AreaIsNoyel(dest.DestArea))
            {
                rewardGold = (int)(rewardGold * 1.8);
            }

            EnsureComp<QuestRewardGoldComponent>(uid).GoldModifier = rewardGold;

            escortQuest.EscortType = escortType;
            escortQuest.TargetMapID = dest.DestMapID;
            escortQuest.TargetMapName = dest.DestQuestHub.MapName;
            escortQuest.EncountersSeen = 0;
        }

        private void QuestEscort_BeforeAccept(EntityUid uid, QuestTypeEscortComponent questEscort, QuestBeforeAcceptEvent args)
        {
            // >>>>>>>> elona122/shade2/chat.hsp:3020 			f=get_freeAlly() ...
            if (!_parties.CanRecruitMoreMembers(_gameSession.Player))
            {
                args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestEscort, "Accept_PartyIsFull");
                args.Cancel();
                return;
            }

            var playerLevel = _levels.GetLevel(_gameSession.Player);
            var playerSpatial = Spatial(_gameSession.Player);
            var map = GetMap(playerSpatial.MapID);
            for (var i = 0; i <= 99; i++)
            {
                EntityUid? chara;
                PrototypeId<EntityPrototype> id;
                var genArgs = EntityGenArgSet.Make(new EntityGenCommonArgs() { IsMapSavable = false });

                if (i == 99)
                    id = Protos.Chara.TownChild;
                else
                    id = _charaGen.PickRandomCharaId(map, genArgs, minLevel: playerLevel + 1, tags: new[] { Protos.Tag.CharaMan });
                chara = _charaGen.GenerateChara(MapCoordinates.Global, id, args: genArgs);

                if (!IsAlive(chara) || !_mapPlacements.TryPlaceChara(chara.Value, playerSpatial.MapPosition) || !_parties.TryRecruitAsAlly(_gameSession.Player, chara.Value))
                {
                    if (IsAlive(chara))
                        EntityManager.DeleteEntity(chara.Value);
                    continue;
                }

                EnsureComp<TemporaryAllyComponent>(chara.Value);
                EnsureComp<EscortedInQuestComponent>(chara.Value).QuestUid = questEscort.Owner;
                questEscort.EscortingChara = chara.Value;
                break;
            }

            if (!IsAlive(questEscort.EscortingChara))
            {
                Logger.ErrorS("quest.escort", "Failed to generate escort character!");
                args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestEscort, "Accept_PartyIsFull");
                args.Cancel();
                return;
            }

            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestClient, "Accept");
            // <<<<<<<< elona122/shade2/chat.hsp:3035 			qParam2(rq)=cId(rc) ...
        }

        private void QuestEscort_OnFailed(EntityUid uid, QuestTypeEscortComponent questEscort, QuestFailedEvent args)
        {
            // >>>>>>>> shade2/quest.hsp:356 			txtMore:txtEf coPurple:txt lang("あなたは護衛の任務を果たせな ..
            if (IsAlive(questEscort.EscortingChara))
            {
                LocaleKey key = new LocaleKey("");
                switch (questEscort.EscortType)
                {
                    case EscortType.Protect:
                        _mes.Display(Loc.GetString("Elona.Quest.Types.Escort.Fail.Dialog.Protect", ("entity", questEscort.EscortingChara)), color: UiColors.MesSkyBlue, entity: questEscort.EscortingChara);
                        key = new LocaleKey("Elona.DamageType.UnseenHand");
                        break;
                    case EscortType.Poison:
                        _mes.Display(Loc.GetString("Elona.Quest.Types.Escort.Fail.Dialog.Poison", ("entity", questEscort.EscortingChara)), color: UiColors.MesSkyBlue, entity: questEscort.EscortingChara);
                        key = new LocaleKey("Elona.DamageType.Poison");
                        break;
                    case EscortType.Deadline:
                        _mes.Display(Loc.GetString("Elona.Quest.Types.Escort.Fail.Dialog.Deadline", ("entity", questEscort.EscortingChara)), color: UiColors.MesSkyBlue, entity: questEscort.EscortingChara);
                        key = new LocaleKey("Elona.DamageType.Burning");
                        break;
                }
                _damages.DamageHP(questEscort.EscortingChara, Math.Max(EnsureComp<SkillsComponent>(questEscort.EscortingChara).HP + 1, 999999), damageType: new GenericDamageType(key));
                EnsureComp<CharaComponent>(questEscort.EscortingChara).Liveness = CharaLivenessState.Dead;
                _parties.RemoveFromCurrentParty(questEscort.EscortingChara);
            }
            _karma.ModifyKarma(_gameSession.Player, -10);
            // <<<<<<<< shade2/quest.hsp:371 			} ..
        }

        private void QuestEscort_GenerateEncounter(MapCalcRandomEncounterIDEvent ev)
        {
            // >>>>>>>> shade2/action.hsp:664 			if rnd(20)=0{ ...
            if (!_rand.OneIn(20))
                return;

            var anyQuests = _quests.EnumerateAcceptedQuests<QuestTypeEscortComponent>()
                .Where(q => q.QuestType.EscortType == EscortType.Protect && q.QuestType.EncountersSeen < 2)
                .Any();

            if (!anyQuests)
                return;

            ev.OutEncounterId = Protos.Encounter.Assassin;
            // <<<<<<<< shade2/action.hsp:670 			} ..
        }
    }
}