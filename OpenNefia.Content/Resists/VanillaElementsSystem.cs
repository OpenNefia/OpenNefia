using OpenNefia.Content.Logic;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Analyzers;

namespace OpenNefia.Content.Resists
{
    public sealed class VanillaElementsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly CommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        #region Elona.Fire

        public void Fire_ModifyDamage(ElementPrototype proto, ref P_ElementModifyDamageEvent ev)
        {
            // >>>>>>>> shade2/chara_func.hsp:1459 		if (ele=rsResFire)or(dmgSource=dmgFromFire):dmg= ..
            if (_effects.HasEffect(ev.Target, Protos.StatusEffect.Wet))
                ev.OutRawDamage /= 3;
            // <<<<<<<< shade2/chara_func.hsp:1459 		if (ele=rsResFire)or(dmgSource=dmgFromFire):dmg= ..
        }

        public void Fire_DamageTile(ElementPrototype proto, ref P_ElementDamageTileEvent ev)
        {
            // >>>>>>>> shade2/proc.hsp:1774 	if ele=rsResFire:mapitem_fire dx,dy ...
            _commonEffects.DamageTileFire(ev.Coords, ev.Source);
            // <<<<<<<< shade2/chara_func.hsp:1459 		if (ele=rsResFire)or(dmgSource=dmgFromFire):dmg= ..
        }

        public void Fire_DamageChara(ElementPrototype proto, ref P_ElementDamageCharaEvent ev)
        {
            // >>>>>>>> shade2/chara_func.hsp:1560 		if (ele=rsResFire)or(dmgSource=dmgFromFire): ite ...
            _commonEffects.DamageItemsFire(ev.Target);
            // <<<<<<<< shade2/chara_func.hsp:1560 		if (ele=rsResFire)or(dmgSource=dmgFromFire): ite ..
        }

        public void Fire_KillChara(ElementPrototype proto, ref P_ElementKillCharaEvent ev)
        {
            // >>>>>>>> shade2/chara_func.hsp:1643 		if (dmgSource=dmgFromFire)or(ele=rsResFire){ ..
            var pos = GetComp<SpatialComponent>(ev.Target).Coordinates;
            var args = EntityGenArgSet.Make(new MefGenArgs()
            {
                TurnDuration = _rand.Next(10) + 5,
                Power = 100,
                Origin = ev.Source
            });
            _entityGen.SpawnEntity(Protos.Mef.Fire, pos, args: args);
            // <<<<<<<< shade2/chara_func.hsp:1645 			} ..
        }

        #endregion
    }

    public class PrototypeEventArgs {}

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PrototypeEventAttribute : Attribute
    {
        public Type PrototypeType { get; }

        public PrototypeEventAttribute(Type prototypeType)
        {
            PrototypeType = prototypeType;
        }
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