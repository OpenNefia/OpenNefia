using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
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
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.Damage;

namespace OpenNefia.Content.Quests
{
    public interface IQuestEliminateTargetSystem : IEntitySystem
    {
        IEnumerable<QuestEliminateTargetComponent> EnumerateQuestEliminateTargets(IMap map, string tag);
        bool AllTargetsEliminated(IMap map, string tag);
    }

    public sealed class QuestEliminateTargetSystem : EntitySystem, IQuestEliminateTargetSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;

        public override void Initialize()
        {
            SubscribeComponent<QuestEliminateTargetComponent, BeforeEntityDeletedEvent>(HandleEntityBeingDeleted);
            SubscribeComponent<QuestEliminateTargetComponent, EntityKilledEvent>(HandleEntityKilled);
        }

        private void HandleEntityBeingDeleted(EntityUid uid, QuestEliminateTargetComponent component, ref BeforeEntityDeletedEvent args)
        {
            if (IsAlive(uid))
                CheckQuestTargetElimination(uid, component);
        }

        private void HandleEntityKilled(EntityUid uid, QuestEliminateTargetComponent component, ref EntityKilledEvent args)
        {
            CheckQuestTargetElimination(uid, component);
        }

        private void CheckQuestTargetElimination(EntityUid uid, QuestEliminateTargetComponent component)
        {
            if (!TryMap(uid, out var map))
                return;

            var targets = EnumerateQuestEliminateTargets(map, component.Tag).ToList();
            var raiseEliminatedEvent = targets.Count == 0
                || targets.Count == 1 && targets[0].Owner == uid;

            var count = targets.Count;
            if (raiseEliminatedEvent)
                count--;

            var ev1 = new MapQuestTargetKilledEvent(component.Tag, int.Max(count, 0));
            RaiseEvent(map.MapEntityUid, ev1);

            if (raiseEliminatedEvent)
            {
                _deferredEvents.Enqueue(() =>
                {
                    var ev2 = new MapQuestTargetsEliminatedEvent(component.Tag);
                    RaiseEvent(map.MapEntityUid, ev2);
                    return TurnResult.Aborted;
                });
            }
        }

        public bool AllTargetsEliminated(IMap map, string tag)
        {
            return EnumerateQuestEliminateTargets(map, tag).Count() == 0;
        }

        public IEnumerable<QuestEliminateTargetComponent> EnumerateQuestEliminateTargets(IMap map, string tag)
        {
            return _lookup.EntityQueryInMap<QuestEliminateTargetComponent>(map)
                 .Where(qet => qet.Tag == tag);
        }
    }

    /// <summary>
    /// Raised when all targets with a specific tag are eliminated.
    /// Raised immediately after the entity is killed/deleted.
    /// </summary>
    [EventUsage(EventTarget.Map)]
    public class MapQuestTargetKilledEvent : EntityEventArgs
    {
        public MapQuestTargetKilledEvent(string tag, int targetsRemaining)
        {
            Tag = tag;
            TargetsRemaining = targetsRemaining;
        }

        public string Tag { get; }
        public int TargetsRemaining { get; }
    }

    /// <summary>
    /// Raised when all targets with a specific tag are eliminated.
    /// Deferred to the beginning of the next turn.
    /// </summary>
    [EventUsage(EventTarget.Map)]
    public class MapQuestTargetsEliminatedEvent : EntityEventArgs
    {
        public MapQuestTargetsEliminatedEvent(string tag)
        {
            Tag = tag;
        }

        public string Tag { get; }
    }
}