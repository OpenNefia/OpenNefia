using OpenNefia.Content.Combat;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
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
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Resists;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Weight;
using OpenNefia.Core.Maths;
using OpenNefia.Content.World;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Karma;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Buffs;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Buffs
{
    public sealed class VanillaBuffsSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IFormulaEngine _formulas = default!;
        [Dependency] private readonly IKarmaSystem _karmas = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IBuffSystem _buffs = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<BuffHolyShieldComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffHolyShield);
            SubscribeComponent<BuffRegenerationComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffRegeneration);
            SubscribeComponent<BuffElementalShieldComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffElementalShield);
            SubscribeComponent<BuffSpeedComponent, BeforeBuffAddedEvent>(AddBuff_BuffSpeed);
            SubscribeComponent<BuffSpeedComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffSpeed);
            SubscribeComponent<BuffSlowComponent, BeforeBuffAddedEvent>(AddBuff_BuffSlow);
            SubscribeComponent<BuffSlowComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffSlow);
            SubscribeComponent<BuffHeroComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffHero);
            SubscribeComponent<BuffMistOfFrailnessComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffMistOfFrailness);
            SubscribeComponent<BuffElementScarComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffElementScar);
            SubscribeComponent<BuffNightmareComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffNightmare);
            SubscribeComponent<BuffDivineWisdomComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffDivineWisdom);
            SubscribeComponent<BuffPunishmentComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffPunishment);
            SubscribeComponent<BuffLulwysTrickComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffLulwysTrick);
            SubscribeComponent<BuffIncognitoComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffIncognito);
            SubscribeComponent<BuffIncognitoComponent, BeforeBuffAddedEvent>(AddBuff_BuffIncognito);
            SubscribeComponent<BuffIncognitoComponent, BeforeBuffRemovedEvent>(RemoveBuff_BuffIncognito);
            SubscribeComponent<BuffDeathWordComponent, BeforeBuffAddedEvent>(AddBuff_BuffDeathWord);
            SubscribeComponent<BuffDeathWordComponent, OnBuffExpiredEvent>(ExpireBuff_BuffDeathWord);
            SubscribeComponent<DeathWordTargetsComponent, EntityKilledEvent>(Killed_DeathWordTargets);
            SubscribeComponent<DeathWordTargetsComponent, BeforeEntityDeletedEvent>(Deleted_DeathWordTargets);
            SubscribeComponent<BuffBoostComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffBoost);
            SubscribeComponent<BuffLuckyComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffLucky);
            SubscribeComponent<BuffFoodComponent, ApplyBuffOnRefreshEvent>(ApplyBuff_BuffFood);

            SubscribeComponent<BuffsComponent, CalcFinalDamageEvent>(ProcContingency, priority: EventPriorities.Low);
        }

        private void ProcContingency(EntityUid uid, BuffsComponent component, ref CalcFinalDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1465 	if cBit(cContingency,tc):if cHp(tc)-dmg<=0:if cal ..
            if (TryComp<SkillsComponent>(uid, out var skills) && skills.HP - args.OutFinalDamage <= 0)
            {
                if (_buffs.TryGetBuff<BuffContingencyComponent>(uid, out var buff))
                {
                    var contingencyPower = buff.Power;
                    if (contingencyPower >= _rand.Next(100))
                    {
                        args.OutFinalDamage = -args.OutFinalDamage;
                    }
                }
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:1465 	if cBit(cContingency,tc):if cHp(tc)-dmg<=0:if cal ..
        }

        private void ApplyBuff_BuffHolyShield(EntityUid uid, BuffHolyShieldComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2683 		cPV(tc)+=p :cFear(tc)=0 ...
            if (TryComp<EquipStatsComponent>(args.Target, out var equipStats))
            {
                equipStats.PV.Buffed += args.Buff.Power;
            }
            _statusEffects.SetTurns(args.Target, Protos.StatusEffect.Fear, 0);
            // <<<<<<<< elona122/shade2/init.hsp:2683 		cPV(tc)+=p :cFear(tc)=0 ...
        }

        private void ApplyBuff_BuffRegeneration(EntityUid uid, BuffRegenerationComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2694 		sHealing(tc)+=40 ...
            _skills.BuffLevel(args.Target, Protos.Skill.Healing, 40);
            // <<<<<<<< elona122/shade2/init.hsp:2694 		sHealing(tc)+=40 ...
        }

        private void ApplyBuff_BuffElementalShield(EntityUid uid, BuffElementalShieldComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2700 		sResFire(tc)+=100 ...
            _resists.BuffLevel(args.Target, Protos.Element.Fire, 100);
            _resists.BuffLevel(args.Target, Protos.Element.Cold, 100);
            _resists.BuffLevel(args.Target, Protos.Element.Lightning, 100);
            // <<<<<<<< elona122/shade2/init.hsp:2702 		sResLightning(tc)+=100 ...
        }

        private void AddBuff_BuffSpeed(EntityUid uid, BuffSpeedComponent component, BeforeBuffAddedEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1676 			if efStatus<=stCursed{ ...
            if (_curseStates.IsCursed(args.CurseState))
            {
                if (TryComp<WeightComponent>(args.Target, out var weight))
                {
                    weight.Age -= _rand.Next(3) + 1;
                    _mes.Display(Loc.GetString("Elona.Buff.Types.Speed.Cursed", ("target", args.Target)), entity: args.Target, color: Color.Purple);
                }
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1679 				} ...
        }

        private void ApplyBuff_BuffSpeed(EntityUid uid, BuffSpeedComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2709 		sSPD(tc)+=p ...
            _skills.BuffLevel(args.Target, Protos.Skill.AttrSpeed, args.Buff.Power);
            // <<<<<<<< elona122/shade2/init.hsp:2709 		sSPD(tc)+=p ...
        }

        private void AddBuff_BuffSlow(EntityUid uid, BuffSlowComponent component, BeforeBuffAddedEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1670 			if efStatus>=stBlessed{ ...
            if (_curseStates.IsBlessed(args.CurseState))
            {
                if (TryComp<WeightComponent>(args.Target, out var weight))
                {
                    weight.Age = Math.Min(weight.Age + _rand.Next(3) + 1, _world.State.GameDate.Year - 12);
                    _mes.Display(Loc.GetString("Elona.Buff.Types.Slow.Blessed", ("target", args.Target)), entity: args.Target, color: Color.Green);
                }
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1673 				} ...
        }

        private void ApplyBuff_BuffSlow(EntityUid uid, BuffSlowComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2716 		sSPD(tc)-=p ...
            _skills.BuffLevel(args.Target, Protos.Skill.AttrSpeed, -args.Buff.Power);
            // <<<<<<<< elona122/shade2/init.hsp:2716 		sSPD(tc)-=p ...
        }

        private void ApplyBuff_BuffHero(EntityUid uid, BuffHeroComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2723 		sSTR(tc)+=p : sDEX(tc)+=p:cFear(tc)=0 : cConfuse ...
            _skills.BuffLevel(args.Target, Protos.Skill.AttrStrength, args.Buff.Power);
            _skills.BuffLevel(args.Target, Protos.Skill.AttrDexterity, args.Buff.Power);
            _statusEffects.SetTurns(args.Target, Protos.StatusEffect.Fear, 0);
            _statusEffects.SetTurns(args.Target, Protos.StatusEffect.Confusion, 0);
            // <<<<<<<< elona122/shade2/init.hsp:2723 		sSTR(tc)+=p : sDEX(tc)+=p:cFear(tc)=0 : cConfuse ...
        }

        private void ApplyBuff_BuffMistOfFrailness(EntityUid uid, BuffMistOfFrailnessComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2730 		cDV(tc)=cDV(tc)/2 ...
            if (TryComp<EquipStatsComponent>(args.Target, out var equipStats))
            {
                equipStats.DV.Buffed /= 2;
                equipStats.PV.Buffed /= 2;
            }
            // <<<<<<<< elona122/shade2/init.hsp:2731 		cPV(tc)=cPV(tc)/2 ...
        }

        private void ApplyBuff_BuffElementScar(EntityUid uid, BuffElementScarComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2737 		sMod sResFire(tc),-100 ...
            _resists.BuffLevel(args.Target, Protos.Element.Fire, -100);
            _resists.BuffLevel(args.Target, Protos.Element.Cold, -100);
            _resists.BuffLevel(args.Target, Protos.Element.Lightning, -100);
            // <<<<<<<< elona122/shade2/init.hsp:2739 		sMod sResLightning(tc),-100 ...
        }

        private void ApplyBuff_BuffNightmare(EntityUid uid, BuffNightmareComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2751 		sMod sResNerve(tc),-100 ...
            _resists.BuffLevel(args.Target, Protos.Element.Nerve, -100);
            _resists.BuffLevel(args.Target, Protos.Element.Mind, -100);
            // <<<<<<<< elona122/shade2/init.hsp:2752 		sMod sResMind(tc),-100 ...
        }

        private void ApplyBuff_BuffDivineWisdom(EntityUid uid, BuffDivineWisdomComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2759 		sLER(tc)+=p : sMAG(tc)+=p:sLiteracy(tc)+=p(1) ...
            var formulaArgs = _buffs.GetBuffFormulaArgs(args.Buff.BasePower);
            formulaArgs["power"] = args.Buff.Power;
            var learningMagic = (int)_formulas.Calculate(comp.LearningMagic, formulaArgs);
            var literacy = (int)_formulas.Calculate(comp.Literacy, formulaArgs);

            _skills.BuffLevel(args.Target, Protos.Skill.AttrLearning, learningMagic);
            _skills.BuffLevel(args.Target, Protos.Skill.AttrMagic, learningMagic);
            _skills.BuffLevel(args.Target, Protos.Skill.Literacy, literacy);
            // <<<<<<<< elona122/shade2/init.hsp:2759 		sLER(tc)+=p : sMAG(tc)+=p:sLiteracy(tc)+=p(1) ...
        }

        private void ApplyBuff_BuffPunishment(EntityUid uid, BuffPunishmentComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2766 		sSPD(tc)-=p ...
            _skills.BuffLevel(args.Target, Protos.Skill.AttrSpeed, -args.Buff.Power);
            if (TryComp<EquipStatsComponent>(args.Target, out var equipStats))
            {
                equipStats.PV.Buffed = int.Max((int)(equipStats.PV.Buffed * comp.PVModifier), 1);
            }
            // <<<<<<<< elona122/shade2/init.hsp:2767 		if cPV(TC)>1:cPV(tc)-=cPV(tc)/5 ...
        }

        private void ApplyBuff_BuffLulwysTrick(EntityUid uid, BuffLulwysTrickComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2775 		sSPD(tc)+=p ...
            _skills.BuffLevel(args.Target, Protos.Skill.AttrSpeed, args.Buff.Power);
            // <<<<<<<< elona122/shade2/init.hsp:2775 		sSPD(tc)+=p ...
        }

        private void AddBuff_BuffIncognito(EntityUid uid, BuffIncognitoComponent component, BeforeBuffAddedEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1681 		if efId=spIncognito : if tc=pc :incognitoBegin ...
            if (_gameSession.IsPlayer(args.Target))
                _karmas.StartIncognito(args.Target);
            // <<<<<<<< elona122/shade2/proc.hsp:1681 		if efId=spIncognito : if tc=pc :incognitoBegin ...
        }

        private void RemoveBuff_BuffIncognito(EntityUid uid, BuffIncognitoComponent component, BeforeBuffRemovedEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:585 	if cBuff(id,tc)=buffIncognito : if tc=pc :incogni ...
            if (_gameSession.IsPlayer(args.Target))
                _karmas.EndIncognito(args.Target);
            // <<<<<<<< elona122/shade2/chara_func.hsp:585 	if cBuff(id,tc)=buffIncognito : if tc=pc :incogni ...
        }

        private void ApplyBuff_BuffIncognito(EntityUid uid, BuffIncognitoComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2781 		cBitMod cIncognito,tc,true ...
            if (TryComp<KarmaComponent>(args.Target, out var karma))
                karma.IsIncognito.Buffed = true;
            // <<<<<<<< elona122/shade2/init.hsp:2781 		cBitMod cIncognito,tc,true ...
        }

        private void AddBuff_BuffDeathWord(EntityUid uid, BuffDeathWordComponent component, BeforeBuffAddedEvent args)
        {
            if (args.Cancelled || !IsAlive(args.Source))
                return;

            var deathWordTargets = EnsureComp<DeathWordTargetsComponent>(args.Source.Value);
            deathWordTargets.Targets.Add(args.Target);
        }

        private void ExpireBuff_BuffDeathWord(EntityUid uid, BuffDeathWordComponent comp, OnBuffExpiredEvent args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:804 			if cBuff(cnt,cc)=buffDeath:dmgHp cc,9999,dmgFro ...
            if (!TryComp<SkillsComponent>(args.Target, out var skills))
                return;
            _damages.DamageHP(uid, Math.Max(9999, skills.MaxHP), damageType: new GenericDamageType("Elona.DamageType.UnseenHand"));
            // <<<<<<<< elona122/shade2/main.hsp:804 			if cBuff(cnt,cc)=buffDeath:dmgHp cc,9999,dmgFro ...
        }

        private void Killed_DeathWordTargets(EntityUid uid, DeathWordTargetsComponent component, ref EntityKilledEvent args)
        {
            RemoveDeathWordFromTargets(uid, component.Targets);
        }

        private void Deleted_DeathWordTargets(EntityUid uid, DeathWordTargetsComponent component, ref BeforeEntityDeletedEvent args)
        {
            RemoveDeathWordFromTargets(uid, component.Targets);
        }

        private void RemoveDeathWordFromTargets(EntityUid source, HashSet<EntityUid> targets)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1749 		if cBit(cDeathMaster,tc)=true{ ...
            var removed = 0;

            foreach (var target in targets.ToList())
            {
                if (!IsAlive(target))
                    continue;

                foreach (var buff in _buffs.EnumerateBuffs(target).ToList())
                {
                    if (HasComp<BuffDeathWordComponent>(buff.Owner) && buff.Source == source)
                    {
                        _buffs.RemoveBuff(target, buff.Owner);
                        removed++;
                    }
                }
            }
            if (removed > 0)
            {
                _mes.Display(Loc.GetString("Elona.Buff.Types.DeathWord.Breaks"));
            }
            targets.Clear();
            // <<<<<<<< elona122/shade2/chara_func.hsp:1759 			} ...
        }

        private void ApplyBuff_BuffBoost(EntityUid uid, BuffBoostComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2796 		sSpd(tc)+=p ...
            _skills.BuffLevel(args.Target, Protos.Skill.AttrSpeed, args.Buff.Power);
            _skills.BuffLevel(args.Target, Protos.Skill.AttrStrength,
                (int)(_skills.Level(args.Target, Protos.Skill.AttrStrength) * 0.5) + 10);
            _skills.BuffLevel(args.Target, Protos.Skill.AttrDexterity,
                (int)(_skills.Level(args.Target, Protos.Skill.AttrDexterity) * 0.5) + 10);
            _skills.BuffLevel(args.Target, Protos.Skill.Healing, 50);
            if (TryComp<EquipStatsComponent>(args.Target, out var equipStats))
            {
                equipStats.PV.Buffed = (int)(equipStats.PV.Buffed * 1.5) + 25;
                equipStats.DV.Buffed = (int)(equipStats.DV.Buffed * 1.5) + 25;
                equipStats.HitBonus.Buffed = (int)(equipStats.HitBonus.Buffed * 1.5) + 50;
            }
            // <<<<<<<< elona122/shade2/init.hsp:2802 		cATK(tc)=cATK(tc)*150/100+50 ...
        }

        private void ApplyBuff_BuffLucky(EntityUid uid, BuffLuckyComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2818 		sLUC(tc)+=p ...
            _skills.BuffLevel(args.Target, Protos.Skill.AttrLuck, args.Buff.Power);
            // <<<<<<<< elona122/shade2/init.hsp:2818 		sLUC(tc)+=p ...
        }

        private void ApplyBuff_BuffFood(EntityUid uid, BuffFoodComponent comp, ref ApplyBuffOnRefreshEvent args)
        {
            // >>>>>>>> elona122/shade2/init.hsp:2675 		cFoodExp(buff-buffFoodSTR+rsSTR,tc)=p ...
            if (!TryComp<GrowthBuffsComponent>(args.Target, out var growthBuffs))
                return;

            growthBuffs.GrowthBuffs[comp.Skill] = args.Buff.Power;
            // <<<<<<<< elona122/shade2/init.hsp:2675 		cFoodExp(buff-buffFoodSTR+rsSTR,tc)=p ...
        }
    }
}
