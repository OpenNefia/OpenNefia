using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Mount;
using NuGet.Packaging.Signing;
using OpenNefia.Content.Weight;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Maps;

namespace OpenNefia.Content.Combat
{
    public sealed partial class CombatSystem
    {
        [Dependency] private readonly IMountSystem _mounts = default!;

        private void BlockPhysicalAttackFear(EntityUid attacker, BeforePhysicalAttackEventArgs args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/action.hsp:1233 *act_attack ..
            if (_effects.HasEffect(attacker, Protos.StatusEffect.Fear))
            {
                _mes.Display(Loc.GetString("Elona.Combat.PhysicalAttack.IsFrightened", ("entity", attacker)), combineDuplicates: true);
                args.Handled = true;
            }
            // <<<<<<<< shade2/action.hsp:1237  ..
        }

        #region Accuracy calculation

        private void HandleCalcAccuracyEquipState(EntityUid attacker, ref CalcPhysicalAttackAccuracyEvent args)
        {
            if (!EntityManager.IsAlive(args.Weapon))
                return;

            if (args.IsRanged)
            {
                if (args.ConsiderDistance && EntityManager.IsAlive(args.Target)
                    && Spatial(attacker).MapPosition.TryDistanceFractional(Spatial(args.Target).MapPosition, out var dist)
                    && TryComp<RangedWeaponComponent>(args.Weapon.Value, out var ranged))
                {
                    args.OutAccuracy = (int)(args.OutAccuracy * ranged.RangedAccuracy.GetAccuracyModifier(dist));
                }
            }
            else
            {
                var weight = CompOrNull<WeightComponent>(args.Weapon)?.Weight.Buffed ?? 0;
                var equipState = GetEquipState(attacker);
                if (equipState.IsWieldingTwoHanded)
                {
                    args.OutAccuracy += 25;
                    if (weight >= WeaponWeight.Heavy)
                    {
                        args.OutAccuracy += _skills.Level(attacker, Protos.Skill.TwoHand);
                    }
                }
                else if (equipState.IsDualWielding)
                {
                    if (args.AttackCount == 1)
                    {
                        if (weight > WeaponWeight.Heavy)
                        {
                            args.OutAccuracy -= (weight - WeaponWeight.Heavy + 400) / (10 + _skills.Level(attacker, Protos.Skill.DualWield) / 5);
                        }
                        else if (weight > WeaponWeight.Light)
                        {
                            args.OutAccuracy -= (weight - WeaponWeight.Light + 100) / (10 + _skills.Level(attacker, Protos.Skill.DualWield) / 5);
                        }
                    }
                }
            }
        }

        private void HandleCalcAccuracyAttackCount(EntityUid attacker, ref CalcPhysicalAttackAccuracyEvent args)
        {
            if (args.AttackCount <= 0)
                return;

            var hits = 100 - (args.AttackCount - 1) * (10000 / (100 + _skills.Level(attacker, Protos.Skill.DualWield) * 10));

            if (args.OutAccuracy > 0)
                args.OutAccuracy = args.OutAccuracy * hits / 100;
        }

        #endregion

        #region Hit state calculation

        private void HandleCalcHitStatusEffects(EntityUid attacker, ref CalcPhysicalAttackHitEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/calculation.hsp:216 	if cDim(tc)!0{ ...
            if (_effects.HasEffect(attacker, Protos.StatusEffect.Dimming))
            {
                if (_rand.OneIn(4))
                {
                    args.Handle(HitResult.CriticalHit);
                    return;
                }
                args.OutEvasion /= 2;
            }
            if (_effects.HasEffect(attacker, Protos.StatusEffect.Blindness))
            {
                args.OutAccuracy /= 2;
            }
            if (_effects.HasEffect(args.Target, Protos.StatusEffect.Blindness))
            {
                args.OutEvasion /= 2;
            }
            if (_effects.HasEffect(args.Target, Protos.StatusEffect.Blindness))
            {
                args.Handle(HitResult.Hit);
                return;
            }
            if (_effects.HasEffect(attacker, Protos.StatusEffect.Confusion) || _effects.HasEffect(attacker, Protos.StatusEffect.Dimming))
            {
                if (args.IsRanged)
                {
                    args.OutAccuracy /= 5;
                }
                else
                {
                    args.OutAccuracy = args.OutAccuracy / 3 * 2;
                }
            }
            // <<<<<<<< shade2/calculation.hsp:225 	if (cConfuse(cc)!0)or(cDim(cc)!0) : if AttackRang ..
        }

        private void HandleCalcHitGreaterEvasion(EntityUid attacker, ref CalcPhysicalAttackHitEvent args)
        {
            // >>>>>>>> shade2/calculation.hsp:227 	if sEvadePlus(tc)!0 : if toHit<sEvadePlus(tc)*10{ ...
            var greaterEvasion = _skills.Level(attacker, Protos.Skill.GreaterEvasion);
            if (greaterEvasion <= 0)
                return;

            if (args.OutAccuracy > 0 && args.OutEvasion < greaterEvasion * 10)
            {
                var evadeRef = args.OutEvasion * 100 / Math.Max(args.OutAccuracy, 1);
                var value = _rand.Next(greaterEvasion * 250);
                if (evadeRef > 300)
                {
                    if (value > 100)
                    {
                        args.Handle(HitResult.Evade);
                        return;
                    }
                }
                else if (evadeRef > 200)
                {
                    if (value > 150)
                    {
                        args.Handle(HitResult.Evade);
                        return;
                    }
                }
                else if (evadeRef > 150)
                {
                    if (value > 200)
                    {
                        args.Handle(HitResult.Evade);
                        return;
                    }
                }
            }
            // <<<<<<<< shade2/calculation.hsp:232 		} ..
        }

        private void HandleCalcHitCriticals(EntityUid attacker, ref CalcPhysicalAttackHitEvent args)
        {
            if (_rand.Next(5000) < _skills.Level(attacker, Protos.Skill.AttrPerception) + 50)
            {
                args.Handle(HitResult.CriticalHit);
                return;
            }

            if (TryComp<EquipStatsComponent>(attacker, out var crit) && crit.CriticalRate.Buffed > _rand.Next(200))
            {
                args.Handle(HitResult.CriticalHit);
                return;
            }

            if (_rand.OneIn(20))
            {
                args.Handle(HitResult.Hit);
                return;
            }

            if (_rand.OneIn(20))
            {
                args.Handle(HitResult.Miss);
                return;
            }
        }

        public int CalcExpDivisor(EntityUid target)
        {
            // TODO move into ECS events
            // TODO make into float percentage modifier
            var divisor = 1;

            if (HasComp<SandBaggedComponent>(target))
                divisor += 15;

            if (TryComp<SplittableComponent>(target, out var splittable))
            {
                if (splittable.SplitsRandomlyWhenAttacked.Buffed)
                    divisor += 1;
                if (splittable.SplitsOnHighDamage.Buffed)
                    divisor += 1;
            }

            if (TryMap(target, out var map) && TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon) && mapCommon.ExperienceDivisor != null)
                divisor += mapCommon.ExperienceDivisor.Value;

            return divisor;
        }

        private void GainCombatSkillExperienceOnHit(EntityUid attacker, SkillsComponent skills, AfterPhysicalAttackHitEventArgs args)
        {
            // >>>>>>>> elona122/shade2/action.hsp:1295 		if critical : skillExp rsCritical,cc,60/expModif ..
            var expDivisor = CalcExpDivisor(args.Target);

            if (args.HitResult == HitResult.CriticalHit)
                _skills.GainSkillExp(attacker, Protos.Skill.EyeOfMind, 60 / expDivisor, 2);

            if (args.RawDamage.TotalDamage > skills.MaxHP / 20
                || args.RawDamage.TotalDamage > _skills.Level(attacker, Protos.Skill.Healing)
                || _rand.OneIn(5))
            {
                var attackSkillExp = Math.Clamp(_skills.Level(args.Target, Protos.Skill.Evasion) * 2 - _skills.Level(attacker, args.AttackSkill) + 1, 5, 50) / expDivisor;
                _skills.GainSkillExp(attacker, args.AttackSkill, attackSkillExp, 0, 4);

                if (args.AttackSkill == Protos.Skill.Throwing)
                {
                    _skills.GainSkillExp(attacker, Protos.Skill.Tactics, 10 / expDivisor, 0, 4);
                }
                else if (args.IsRanged)
                {
                    _skills.GainSkillExp(attacker, Protos.Skill.Tactics, 25 / expDivisor, 0, 4);
                }
                else
                {
                    _skills.GainSkillExp(attacker, Protos.Skill.Tactics, 20 / expDivisor, 0, 4);

                    var equipState = GetEquipState(attacker);
                    if (equipState.IsWieldingTwoHanded)
                        _skills.GainSkillExp(attacker, Protos.Skill.TwoHand, 20 / expDivisor, 0, 4);
                    if (equipState.IsDualWielding)
                        _skills.GainSkillExp(attacker, Protos.Skill.DualWield, 20 / expDivisor, 0, 4);
                }

                if (_mounts.HasMount(attacker))
                    _skills.GainSkillExp(attacker, Protos.Skill.Riding, 30 / expDivisor, 0, 5);

                if (IsAlive(args.Target) && TryComp<SkillsComponent>(args.Target, out var targetSkills))
                {
                    var armorClass = _equip.GetArmorClass(args.Target);
                    var armorClassExp = Math.Clamp(250 * args.RawDamage.TotalDamage / targetSkills.MaxHP + 1, 3, 100) / expDivisor;
                    _skills.GainSkillExp(args.Target, armorClass, armorClassExp, 0, 5);

                    var equipState = GetEquipState(args.Target);
                    if (equipState.IsWieldingShield)
                        _skills.GainSkillExp(args.Target, Protos.Skill.Shield, 40 / expDivisor, 0, 4);
                }
            }
            // <<<<<<<< elona122/shade2/action.hsp:1312 		} ..
        }

        private void GainCombatSkillExperienceOnMiss(EntityUid attacker, SkillsComponent skills, AfterPhysicalAttackMissEventArgs args)
        {
            // >>>>>>>> shade2/action.hsp:1356 		if (sdata(attackSkill,cc)>sEvade(tc))or(rnd(5)=0 ..
            var expDivisor = CalcExpDivisor(args.Target);
            var attackerSkillLevel = _skills.Level(attacker, args.AttackSkill);
            var targetEvasionLevel = _skills.Level(args.Target, Protos.Skill.Evasion);
            if (attackerSkillLevel > targetEvasionLevel || _rand.OneIn(5))
            {
                var exp = Math.Clamp(attackerSkillLevel - targetEvasionLevel / 2 + 1, 1, 20) / expDivisor;
                _skills.GainSkillExp(args.Target, Protos.Skill.Evasion, exp, 0, 4);
                _skills.GainSkillExp(args.Target, Protos.Skill.GreaterEvasion, exp, 0, 4);
            }
            // <<<<<<<< shade2/action.hsp:1360 			} ..
        }

        #endregion
    }
}
