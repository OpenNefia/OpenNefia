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

namespace OpenNefia.Content.Quests
{
    public interface IMapImmediateQuestSystem : IEntitySystem
    {

        /// <summary>
        /// Sets an immediate quest for this map and begins the timer.
        /// </summary>
        void SetImmediateQuest(IMap map, QuestComponent quest, GameTimeSpan duration, MapEntrance prevLocation, MapImmediateQuestComponent? comp = null);

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

        public const string MapTimerID = "Elona.ImmediateQuest";

        public override void Initialize()
        {
            SubscribeComponent<MapImmediateQuestComponent, MapOnTimePassedEvent>(UpdateMapImmediateQuest);
            SubscribeComponent<MapImmediateQuestComponent, MapTimerExpiredEvent>(FinishMapImmediateQuest);
        }

        public void SetImmediateQuest(IMap map, QuestComponent quest, GameTimeSpan duration, MapEntrance prevLocation, MapImmediateQuestComponent? comp = null)
        {
            if (!Resolve(map.MapEntityUid, ref comp, logMissing: false))
                comp = EnsureComp<MapImmediateQuestComponent>(map.MapEntityUid);

            comp.QuestUid = quest.Owner;
            comp.PreviousLocation = prevLocation;
            comp.TimeToNextNotify = GameTimeSpan.FromMinutes(10);
            _mapTimers.AddOrUpdateTimer(map, MapTimerID, duration);
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