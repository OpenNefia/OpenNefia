using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Maps
{
    public interface IMapTimersSystem : IEntitySystem
    {
        MapTimer AddOrUpdateTimer(IMap map, string id, GameTimeSpan duration, MapTimersComponent? mapTimers = null);
        void RemoveTimer(IMap map, string id, GameTimeSpan duration, bool raiseEvent = true, MapTimersComponent? mapTimers = null);
        bool TryGetTimer(IMap map, string id, [NotNullWhen(true)] out MapTimer? timer, MapTimersComponent? mapTimers = null);
        MapTimer GetTimer(IMap map, string id, MapTimersComponent? mapTimers = null);
    }

    public sealed class MapTimersSystem : EntitySystem, IMapTimersSystem
    {
        public override void Initialize()
        {
            SubscribeComponent<MapTimersComponent, MapOnTimePassedEvent>(UpdateMapTimers, priority: EventPriorities.VeryHigh);
        }

        private void UpdateMapTimers(EntityUid mapUid, MapTimersComponent component, ref MapOnTimePassedEvent args)
        {
            foreach (var (id, timer) in component.Timers.ToList())
            {
                timer.TimeRemaining -= args.TotalTimePassed;
                if (timer.TimeRemaining < GameTimeSpan.Zero)
                {
                    var ev = new MapTimerExpiredEvent(id);
                    RaiseEvent(mapUid, ev);
                    component.Timers.Remove(id);
                }
            }
        }

        public MapTimer AddOrUpdateTimer(IMap map, string id, GameTimeSpan duration, MapTimersComponent? mapTimers = null)
        {
            if (!Resolve(map.MapEntityUid, ref mapTimers, logMissing: false))
                mapTimers = EnsureComp<MapTimersComponent>(map.MapEntityUid);

            var timer = mapTimers.Timers.GetOrInsertNew(id);
            timer.TimeRemaining = duration;
            return timer;
        }

        public void RemoveTimer(IMap map, string id, GameTimeSpan duration, bool raiseEvent = true, MapTimersComponent? mapTimers = null)
        {
            if (!Resolve(map.MapEntityUid, ref mapTimers, logMissing: false))
                mapTimers = EnsureComp<MapTimersComponent>(map.MapEntityUid);

            if (!mapTimers.Timers.ContainsKey(id))
                return;

            if (raiseEvent)
            {
                var ev = new MapTimerExpiredEvent(id);
                RaiseEvent(map.MapEntityUid, ev);
            }

            mapTimers.Timers.Remove(id);
        }

        public bool TryGetTimer(IMap map, string id, [NotNullWhen(true)] out MapTimer? timer, MapTimersComponent? mapTimers = null)
        {
            if (!Resolve(map.MapEntityUid, ref mapTimers, logMissing: false))
            {
                timer = null;
                return false;
            }

            return mapTimers.Timers.TryGetValue(id, out timer);
        }

        public MapTimer GetTimer(IMap map, string id, MapTimersComponent? mapTimers = null)
        {
            if (!Resolve(map.MapEntityUid, ref mapTimers, logMissing: false))
                mapTimers = EnsureComp<MapTimersComponent>(map.MapEntityUid);

            return mapTimers.Timers[id];
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class MapTimerExpiredEvent : EntityEventArgs
    {
        public MapTimerExpiredEvent(string timerID)
        {
            TimerID = timerID;
        }

        public string TimerID { get; }
    }
}