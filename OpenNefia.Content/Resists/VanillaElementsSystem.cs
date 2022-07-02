using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Prototypes;

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
            var pos = Comp<SpatialComponent>(ev.Target).Coordinates;
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

        #region Elona.Magic

        public void Magic_CalcInitialResistLevel(ElementPrototype proto, ref P_ElementCalcInitialResistLevel ev)
        {
            // >>>>>>>> shade2/calculation.hsp:979 	if ((cnt=rsResMagic)&(p<500))or(cLevel(r1)=1):p=1 ..
            if (ev.OutInitialLevel < 500)
                ev.OutInitialLevel = 100;
            // <<<<<<<< shade2/calculation.hsp:979 	if ((cnt=rsResMagic)&(p<500))or(cLevel(r1)=1):p=1 ..
        }

        #endregion
    }
}