using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Resists
{
    public interface IResistsSystem : IEntitySystem
    {
        int Level(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null);
        int Level(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null);
        int Grade(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null);
        int Grade(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null);

        bool TryGetKnown(EntityUid uid, PrototypeId<ElementPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, ResistsComponent? resists = null);

        bool HasResist(EntityUid uid, PrototypeId<ElementPrototype> protoId, ResistsComponent? resists = null);
        bool HasResist(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null);

        /// <summary>
        /// Enumerates all resistable elemental damage types.
        /// </summary>
        IEnumerable<ElementPrototype> EnumerateResistableElements();
    }

    public sealed partial class ResistsSystem : EntitySystem, IResistsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;

        public override void Initialize()
        {
            SubscribeComponent<ResistsComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ResistsComponent, EntityBeingGeneratedEvent>(CalcInitialResistanceLevels, priority: EventPriorities.VeryHigh);
        }
        
        private void HandleRefresh(EntityUid uid, ResistsComponent resists, ref EntityRefreshEvent args)
        {
            ResetResistBuffs(resists);
        }

        private void CalcInitialResistanceLevels(EntityUid uid, ResistsComponent component, ref EntityBeingGeneratedEvent args)
        {
            foreach (var element in EnumerateResistableElements())
            {
                var level = CalcInitialResistanceLevel(uid, element, component);
                component.Ensure(element).Level.Base = level;
            }
        }

        private int CalcInitialResistanceLevel(EntityUid uid, ElementPrototype element, ResistsComponent component)
        {
            // >>>>>>>> shade2/calculation.hsp:976 	repeat tailResist-headResist,headResist ..
            if (_gameSession.IsPlayer(uid))
                return 100;

            var initialLevel = component.Ensure(element).Level.Base;
            var newLevel = Math.Min(_levels.GetLevel(uid) * 4 + 96, 300);
            if (initialLevel != 0)
            {
                if (initialLevel < 100 || initialLevel > 500)
                {
                    newLevel = initialLevel;
                }
                else
                {
                    newLevel += initialLevel;
                }
            }

            var ev = new P_ElementCalcInitialResistLevel(uid, newLevel);
            _protos.EventBus.RaiseEvent(element, ev);
            newLevel = ev.OutInitialLevel;

            return newLevel;
            // <<<<<<<< shade2/calculation.hsp:981 	loop ..
        }

        private void ResetResistBuffs(ResistsComponent resists)
        {
            foreach (var (_, level) in resists.Resists)
            {
                level.Level.Reset();
            }
        }

        public bool TryGetKnown(EntityUid uid, PrototypeId<ElementPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
            {
                level = null;
                return false;
            }

            return resists.TryGetKnown(protoId, out level);
        }

        public bool HasResist(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null)
            => HasResist(uid, proto.GetStrongID(), resists);

        public bool HasResist(EntityUid uid, PrototypeId<ElementPrototype> protoId, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
                return false;

            return resists.TryGetKnown(protoId, out _);
        }

        public int Level(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null)
            => Level(uid, proto.GetStrongID(), resists);
        public int Level(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
            {
                return 0;
            }

            return resists.Level(id);
        }

        public int Grade(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null)
            => Grade(uid, proto.GetStrongID(), resists);
        public int Grade(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
            {
                return 0;
            }

            return resists.Grade(id);
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(ElementPrototype))]
    public struct P_ElementCalcInitialResistLevel
    {
        public P_ElementCalcInitialResistLevel(EntityUid entity, int initialLevel)
        {
            Entity = entity;
            OutInitialLevel = initialLevel;
        }

        public EntityUid Entity { get; }

        public int OutInitialLevel { get; set; }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(ElementPrototype))]
    public struct P_ElementModifyDamageEvent
    {
        public P_ElementModifyDamageEvent(EntityUid target)
        {
            Target = target;
        }

        public EntityUid Target { get; }

        public int OutRawDamage { get; set; } = 0;
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(ElementPrototype))]
    public struct P_ElementDamageTileEvent
    {
        public EntityUid? Source { get; }
        public MapCoordinates Coords { get; }

        public P_ElementDamageTileEvent(MapCoordinates coords, EntityUid? source)
        {
            Coords = coords;
            Source = source;
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(ElementPrototype))]
    public struct P_ElementDamageCharaEvent
    {
        public EntityUid? Source { get; }
        public EntityUid Target { get; }

        public P_ElementDamageCharaEvent(EntityUid? source, EntityUid target)
        {
            Source = source;
            Target = target;
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(ElementPrototype))]
    public struct P_ElementKillCharaEvent
    {
        public EntityUid? Source { get; }
        public EntityUid Target { get; }

        public P_ElementKillCharaEvent(EntityUid? source, EntityUid target)
        {
            Source = source;
            Target = target;
        }
    }
}