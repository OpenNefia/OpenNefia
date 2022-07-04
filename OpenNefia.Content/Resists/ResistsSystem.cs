using System;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Damage;

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
            SubscribeEntity<CalcFinalDamageEvent>(ApplyElementalDamage, priority: EventPriorities.VeryHigh + 20000);
            SubscribeEntity<AfterDamageAppliedEvent>(ApplyElementOnDamageEvents, priority: EventPriorities.High);
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

        private void ApplyElementalDamage(EntityUid uid, ref CalcFinalDamageEvent args)
        {
            if (args.DamageType is not ElementalDamageType ele)
                return;

            var resists = CompOrNull<ResistsComponent>(uid);

            // >>>>>>>> elona122/shade2/chara_func.hsp:1463 	if ele:if ele!rsResMagic:if cBit(cResEle,tc):dmg= ..
            if (resists != null && resists.IsImmuneToElementalDamage)
            {
                args.OutFinalDamage = 0;
                return;
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:1463 	if ele:if ele!rsResMagic:if cBit(cResEle,tc):dmg= ..

            var eleProto = _protos.Index(ele.ElementID);

            // >>>>>>>> shade2/chara_func.hsp:1444 	if (ele=false)or(ele>=tailResist){ ..
            if (eleProto.CanResist && resists != null)
            {
                var grade = Grade(uid, ele.ElementID, resists);
                if (grade < ResistGrades.Minimum)
                    args.OutFinalDamage = args.OutFinalDamage * 150 / (Math.Clamp(grade * 50 + 50, 40, 150));
                else if (grade < ResistGrades.Immune)
                    args.OutFinalDamage = args.OutFinalDamage * 100 / (grade * 50 + 50);
                else
                    args.OutFinalDamage = 0;
                args.OutFinalDamage = args.OutFinalDamage + 100 / (Level(uid, Protos.Element.Magic, resists) / 2 + 50);
            }
            // <<<<<<<< shade2/chara_func.hsp:1454 		} ..

            // >>>>>>>> elona122/shade2/chara_func.hsp:1458 	if cWet(tc)>0{ ..
            var ev = new P_ElementModifyDamageEvent(uid, args.BaseDamage, args.OutFinalDamage, args.Attacker, ele, args.ExtraArgs);
            _protos.EventBus.RaiseEvent(ele.ElementID, ref ev);
            args.OutFinalDamage = ev.OutFinalDamage;
            // <<<<<<<< elona122/shade2/chara_func.hsp:1461 		} ..

            // >>>>>>>> elona122/shade2/chara_func.hsp:1463 	if ele:if ele!rsResMagic:if cBit(cResEle,tc):dmg= ..
            // <<<<<<<< elona122/shade2/chara_func.hsp:1463 	if ele:if ele!rsResMagic:if cBit(cResEle,tc):dmg= ..
        }

        private void ApplyElementOnDamageEvents(EntityUid uid, ref AfterDamageAppliedEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1541 		if ele{ ...
            if (args.DamageType is not ElementalDamageType ele)
                return;

            var ev = new P_ElementDamageCharaEvent(uid, args.BaseDamage, args.FinalDamage, args.Attacker, ele, args.ExtraArgs);
            _protos.EventBus.RaiseEvent(ele.ElementID, ref ev);
            // <<<<<<<< shade2/chara_func.hsp:1558 			} ..
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
        public EntityUid Target { get; }
        public int BaseDamage { get; }
        public EntityUid? Attacker { get; }
        public IDamageType DamageType { get; }
        public DamageHPExtraArgs? ExtraArgs { get; }

        public int OutFinalDamage { get; set; }

        public P_ElementModifyDamageEvent(EntityUid target, int damage, int finalDamage, EntityUid? attacker, IDamageType damageType, DamageHPExtraArgs? extraArgs)
        {
            Target = target;
            BaseDamage = damage;
            Attacker = attacker;
            DamageType = damageType;
            ExtraArgs = extraArgs;

            OutFinalDamage = finalDamage;
        }
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
        public EntityUid Target { get; }
        public int BaseDamage { get; }
        public int FinalDamage { get; }
        public EntityUid? Attacker { get; }
        public IDamageType DamageType { get; }
        public DamageHPExtraArgs? ExtraArgs { get; }

        public P_ElementDamageCharaEvent(EntityUid target, int damage, int finalDamage, EntityUid? attacker, IDamageType damageType, DamageHPExtraArgs? extraArgs)
        {
            Target = target;
            BaseDamage = damage;
            FinalDamage = finalDamage;
            Attacker = attacker;
            DamageType = damageType;
            ExtraArgs = extraArgs;
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