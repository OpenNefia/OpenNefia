using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Mount;

namespace OpenNefia.Content.Combat
{
    public sealed partial class CombatSystem
    {
        public override void Initialize()
        {
            SubscribeEntity<BeforePhysicalAttackEventArgs>(BlockPhysicalAttackFear, priority: EventPriorities.VeryHigh);

            SubscribeEntity<CalcPhysicalAttackAccuracyEvent>(HandleCalcAccuracyAttackCount, priority: EventPriorities.VeryHigh);

            SubscribeEntity<CalcPhysicalAttackHitEvent>(HandleCalcHitStatusEffects, priority: EventPriorities.VeryHigh);
            SubscribeEntity<CalcPhysicalAttackHitEvent>(HandleCalcHitGreaterEvasion, priority: EventPriorities.VeryHigh);
            SubscribeEntity<CalcPhysicalAttackHitEvent>(HandleCalcHitCriticals, priority: EventPriorities.VeryHigh);
        }

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

        private void HandleCalcAccuracyAttackCount(EntityUid attacker, ref CalcPhysicalAttackAccuracyEvent args)
        {
            if (args.AttackCount <= 0)
                return;

            var hits = 100 - (args.AttackCount - 1) * (10000 / (100 * _skills.Level(attacker, Protos.Skill.DualWield) * 10));
            
            if (args.OutToHit > 0)
                args.OutToHit = args.OutToHit * hits / 100;
        }

        private void HandleCalcEvasion(EntityUid target, CalcPhysicalAttackEvasionEvent args)
        {
            args.OutEvasion = _skills.Level(target, Protos.Skill.AttrPerception) / 3;
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
                args.OutToHit /= 2;
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
                    args.OutToHit /= 5;
                }
                else
                {
                    args.OutToHit = args.OutToHit / 3 * 2;
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

            if (args.OutToHit > 0 && args.OutEvasion < greaterEvasion * 10)
            {
                var evadeRef = args.OutEvasion * 100 / Math.Max(args.OutToHit, 1);
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
