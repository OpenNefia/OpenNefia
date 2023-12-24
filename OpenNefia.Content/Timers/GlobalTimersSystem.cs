using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Timers
{
    public interface IGlobalTimersSystem : IEntitySystem
    {
        GlobalTimer AddOrUpdateTimer(string id, GameTimeSpan duration, TimerUpdateType type);
        void RemoveTimer(string id, bool raiseEvent = true);
        bool TryGetTimer(string id, [NotNullWhen(true)] out GlobalTimer? timer);
        GlobalTimer GetTimer(string id);
    }

    public sealed class GlobalTimersSystem : EntitySystem, IGlobalTimersSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        [RegisterSaveData("Elona.GlobalTimersSystem.GlobalTimers")]
        private Dictionary<string, GlobalTimer> _globalTimers { get; } = new();

        private List<string> _expiredTimerIDs = new();

        public override void Initialize()
        {
            SubscribeBroadcast<MapBeforeTurnBeginEventArgs>(UpdateGlobalTimers_MapTurns, priority: EventPriorities.VeryHigh - 1000);
            SubscribeBroadcast<MapOnTimePassedEvent>(UpdateGlobalTimers_InGameTime, priority: EventPriorities.VeryHigh - 1000);
        }

        private void UpdateGlobalTimers_MapTurns(MapBeforeTurnBeginEventArgs args)
        {
            foreach (var (id, timer) in _globalTimers.ToList())
            {
                if (timer.UpdateType != TimerUpdateType.MapTurnsPassed)
                    continue;

                timer.TimeRemaining -= GameTimeSpan.FromMinutes(1);
                if (timer.TimeRemaining < GameTimeSpan.Zero)
                {
                    _globalTimers.Remove(id);
                    _expiredTimerIDs.Add(id);
                }
            }

            // For consistency, defer running timers until the start of the map turn.
            foreach (var timerID in _expiredTimerIDs)
            {
                var ev = new GlobalTimerExpiredEvent(timerID);
                RaiseEvent(ev);
            }

            _expiredTimerIDs.Clear();
        }

        private void UpdateGlobalTimers_InGameTime(ref MapOnTimePassedEvent args)
        {
            foreach (var (id, timer) in _globalTimers.ToList())
            {
                if (timer.UpdateType != TimerUpdateType.TimePassed)
                    continue;

                timer.TimeRemaining -= args.TotalTimePassed;
                if (timer.TimeRemaining < GameTimeSpan.Zero)
                {
                    _globalTimers.Remove(id);
                    _expiredTimerIDs.Add(id);
                }
            }
        }

        public GlobalTimer AddOrUpdateTimer(string id, GameTimeSpan duration, TimerUpdateType type)
        {
            var timer = _globalTimers.GetOrInsertNew(id);
            timer.TimeRemaining = duration;
            timer.UpdateType = type;
            return timer;
        }

        public void RemoveTimer(string id, bool raiseEvent = true)
        {
            if (!_globalTimers.ContainsKey(id))
                return;

            if (raiseEvent)
            {
                var ev = new GlobalTimerExpiredEvent(id);
                RaiseEvent(ev);
            }

            _globalTimers.Remove(id);
        }

        public bool TryGetTimer(string id, [NotNullWhen(true)] out GlobalTimer? timer)
        {
            return _globalTimers.TryGetValue(id, out timer);
        }

        public GlobalTimer GetTimer(string id)
        {
            return _globalTimers[id];
        }
    }

    public enum TimerUpdateType
    {
        /// <summary>
        /// The timer's <see cref="GlobalTimer.TimeRemaining"/> will be decremented
        /// by one minute for every turn that passes in the map (per turn ordering,
        /// not in-game time).
        /// The timer will update consistently no matter the player's speed.
        /// </summary>
        MapTurnsPassed,

        /// <summary>
        /// The amount of in-game time that passes will be decremented from
        /// the timer's <see cref="GlobalTimer.TimeRemaining"/>.
        /// The timer will update faster if the player's speed is high.
        /// </summary>
        TimePassed,
    }

    [DataDefinition]
    public class GlobalTimer
    {
        /// <summary>
        /// Time left.
        /// </summary>
        [DataField]
        public GameTimeSpan TimeRemaining { get; set; } = GameTimeSpan.Zero;

        /// <summary>
        /// How time should be decremented from this timer.
        /// </summary>
        [DataField]
        public TimerUpdateType UpdateType { get; set; }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class GlobalTimerExpiredEvent : EntityEventArgs
    {
        public GlobalTimerExpiredEvent(string timerID)
        {
            TimerID = timerID;
        }

        public string TimerID { get; }
    }
}