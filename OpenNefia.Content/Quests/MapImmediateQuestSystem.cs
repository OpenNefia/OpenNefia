using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Maps;
using OpenNefia.Content.UI;
using OpenNefia.Core.Log;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.Encounters;
using OpenNefia.Content.GameObjects;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Quests;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Charas;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// An "immediate quest" is a quest associated with the current map. If the player
    /// attempts to leave the map before the quest is marked <see cref="QuestState.Completed"/>,
    /// then the quest will be automatically failed.
    /// Immediate quests may also have an optional duration, after which custom code
    /// can be run by subscribing to the <see cref="QuestTimerExpiredEvent"/>
    /// </summary>
    public interface IMapImmediateQuestSystem : IEntitySystem
    {
        /// <summary>
        /// Sets an immediate quest for this map and begins the timer.
        /// </summary>
        void SetImmediateQuest(IMap map, QuestComponent quest, MapEntrance prevLocation, GameTimeSpan? duration = null, MapImmediateQuestComponent? comp = null);

        bool TryGetImmediateQuest(IMap map, [NotNullWhen(true)] out QuestComponent? quest, [NotNullWhen(true)] out MapImmediateQuestComponent? immediateQuest);
        bool TryGetImmediateQuest<T>(IMap map, [NotNullWhen(true)] out QuestComponent? quest, [NotNullWhen(true)] out MapImmediateQuestComponent? immediateQuest, [NotNullWhen(true)] out T? questComp) where T : class, IComponent;
        bool HasImmediateQuest<T>(IMap map) where T : class, IComponent;
        GameTimeSpan GetImmediateQuestRemainingTime(IMap map);
    }

    public sealed class MapImmediateQuestSystem : EntitySystem, IMapImmediateQuestSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMapTimersSystem _mapTimers = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IPlayerQuery _playerQueries = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ITurnOrderSystem _turnOrders = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;

        public const string MapTimerID = "Elona.ImmediateQuest";

        public override void Initialize()
        {
            SubscribeComponent<MapImmediateQuestComponent, MapOnTimePassedEvent>(UpdateMapImmediateQuest);
            SubscribeComponent<MapImmediateQuestComponent, MapTimerExpiredEvent>(FinishMapImmediateQuest);
            SubscribeComponent<MapImmediateQuestComponent, BeforeExitMapFromEdgesEventArgs>(BeforeExitEdges_CheckAbandonment);
            SubscribeComponent<MapImmediateQuestComponent, BeforeMapLeaveEventArgs>(BeforeMapLeave_CheckAbandonment);
            SubscribeComponent<MapImmediateQuestComponent, PlayerDiedEventArgs>(PlayerDied_FailQuest);
        }

        private void BeforeExitEdges_CheckAbandonment(EntityUid uid, MapImmediateQuestComponent comp, BeforeExitMapFromEdgesEventArgs args)
        {
            // >>>>>>>> elona122/shade2/action.hsp:583 		if mType=mTypeQuest:if gQuestStatus!qSuccess:txt ...
            if (TryGetImmediateQuest(args.Map, out var quest, out _) && quest.State != QuestState.Completed)
                _mes.Display(Loc.GetString("Elona.Quest.AboutToAbandon"));
            // <<<<<<<< elona122/shade2/action.hsp:583 		if mType=mTypeQuest:if gQuestStatus!qSuccess:txt ...
        }

        private void BeforeMapLeave_CheckAbandonment(EntityUid uid, MapImmediateQuestComponent component, BeforeMapLeaveEventArgs args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:322 	if gQuestStatus!qSuccess{ ...
            if (TryGetImmediateQuest(args.OldMap, out var quest, out _) && quest.State != QuestState.Completed)
            {
                _mes.Display(Loc.GetString("Elona.Quest.LeftYourClient"));
                _quests.FailQuest(component.QuestUid);
                _playerQueries.PromptMore();
            }
            // <<<<<<<< elona122/shade2/quest.hsp:327 		} ...
        }

        private void PlayerDied_FailQuest(EntityUid uid, MapImmediateQuestComponent component, PlayerDiedEventArgs args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:1509 	if gQuest:goto *quest_death ...
            DebugTools.Assert(_charas.Revive(_gameSession.Player, force: true), "Failed to revive player!");
            _skills.GainSkillExp(_gameSession.Player, Protos.Skill.AttrCharisma, -500);
            _skills.GainSkillExp(_gameSession.Player, Protos.Skill.AttrWill, -500);
            _refresh.Refresh(_gameSession.Player);
            _mapEntrances.UseMapEntrance(_gameSession.Player, component.PreviousLocation);
            args.Handle(TurnResult.Aborted);
            // <<<<<<<< elona122/shade2/main.hsp:1509 	if gQuest:goto *quest_death ...
        }

        public void SetImmediateQuest(IMap map, QuestComponent quest, MapEntrance prevLocation, GameTimeSpan? duration, MapImmediateQuestComponent? comp = null)
        {
            if (!Resolve(map.MapEntityUid, ref comp, logMissing: false))
                comp = EnsureComp<MapImmediateQuestComponent>(map.MapEntityUid);

            comp.QuestUid = quest.Owner;
            comp.PreviousLocation = prevLocation;

            if (duration != null)
            {
                comp.TimeToNextNotify = GameTimeSpan.FromMinutes(10);
                _mapTimers.AddOrUpdateTimer(map, MapTimerID, duration.Value);
            }
        }

        private void UpdateMapImmediateQuest(EntityUid uid, MapImmediateQuestComponent component, ref MapOnTimePassedEvent args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:577 		if gTimeLimit>0{ ...
            if (!_mapTimers.TryGetTimer(args.Map, MapTimerID, out var timer))
                return;

            component.TimeToNextNotify -= args.TotalTimePassed;
            if (component.TimeToNextNotify <= GameTimeSpan.Zero)
            {
                _mes.Display(Loc.GetString("Elona.Quest.MinutesLeft", ("minutesLeft", timer.TimeRemaining.TotalMinutes + 1)), color: UiColors.MesSkyBlue);
                component.TimeToNextNotify = GameTimeSpan.FromMinutes(10);
            }
            // <<<<<<<< elona122/shade2/main.hsp:581 			} ...
        }

        private void FinishMapImmediateQuest(EntityUid uid, MapImmediateQuestComponent component, MapTimerExpiredEvent args)
        {
            if (args.TimerID != MapTimerID)
                return;

            if (!IsAlive(component.QuestUid) || !TryComp<QuestComponent>(component.QuestUid, out var quest))
            {
                Logger.ErrorS("quest.immediate", $"Immediate quest map component did not contain valid quest!");
                return;
            }

            Logger.InfoS("quest.immediate", $"Immediate quest finished.");

            var map = GetMap(component.Owner);
            _deferredEvents.Enqueue(() =>
            {
                var ev = new QuestTimerExpiredEvent(map, quest, component);
                RaiseEvent(component.QuestUid, ev);
                return TurnResult.Aborted;
            });
        }

        public bool TryGetImmediateQuest(IMap map, [NotNullWhen(true)] out QuestComponent? quest, [NotNullWhen(true)] out MapImmediateQuestComponent? immediateQuest)
        {
            if (!TryComp<MapImmediateQuestComponent>(map.MapEntityUid, out immediateQuest))
            {
                quest = null;
                return false;
            }

            if (!IsAlive(immediateQuest.QuestUid) || !TryComp<QuestComponent>(immediateQuest.QuestUid, out quest))
            {
                quest = null;
                immediateQuest = null;
                return false;
            }

            return true;
        }

        public bool TryGetImmediateQuest<T>(IMap map, [NotNullWhen(true)] out QuestComponent? quest, [NotNullWhen(true)] out MapImmediateQuestComponent? immediateQuest, [NotNullWhen(true)] out T? questComp) where T : class, IComponent
        {
            if (!TryGetImmediateQuest(map, out quest, out immediateQuest))
            {
                questComp = null;
                return false;
            }

            return TryComp<T>(quest.Owner, out questComp);
        }

        public bool HasImmediateQuest<T>(IMap map) where T : class, IComponent
            => TryGetImmediateQuest<T>(map, out _, out _, out _);

        public GameTimeSpan GetImmediateQuestRemainingTime(IMap map)
        {
            if (!_mapTimers.TryGetTimer(map, MapTimerID, out var timer))
                return GameTimeSpan.Zero;

            return timer.TimeRemaining;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestTimerExpiredEvent
    {
        public QuestTimerExpiredEvent(IMap map, QuestComponent quest, MapImmediateQuestComponent immediateQuest)
        {
            Map = map;
            Quest = quest;
            ImmediateQuest = immediateQuest;
        }

        public IMap Map { get; }
        public QuestComponent Quest { get; }
        public MapImmediateQuestComponent ImmediateQuest { get; }
    }
}