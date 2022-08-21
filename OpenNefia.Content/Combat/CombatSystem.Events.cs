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

namespace OpenNefia.Content.Combat
{
    public sealed partial class CombatSystem
    {
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

        #endregion
    }
}
