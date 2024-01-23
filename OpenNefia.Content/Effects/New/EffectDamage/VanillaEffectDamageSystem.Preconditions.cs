using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Effects.New.EffectDamage
{
    public sealed partial class VanillaEffectDamageSystem 
    {
        private void Initialize_Preconditions()
        {
            SubscribeComponent<EffectBaseDamageDiceComponent, BeforeApplyEffectDamageEvent>(ApplyDamage_Dice, priority: EventPriorities.VeryHigh - 10000);
            SubscribeComponent<EffectDamageRetargetComponent, BeforeApplyEffectDamageEvent>(ApplyDamage_Retarget, priority: EventPriorities.VeryHigh - 5000);
            SubscribeComponent<EffectDamageCastInsteadComponent, BeforeApplyEffectDamageEvent>(ApplyDamage_CastInstead, priority: EventPriorities.VeryHigh - 4000);
            SubscribeComponent<EffectDamageRelationsComponent, BeforeApplyEffectDamageEvent>(ApplyDamage_Relations, priority: EventPriorities.VeryHigh - 3000);
            SubscribeComponent<EffectDamageSuccessRateComponent, BeforeApplyEffectDamageEvent>(ApplyDamage_SuccessRate, priority: EventPriorities.VeryHigh - 2000);
            SubscribeComponent<EffectDamageControlMagicComponent, BeforeApplyEffectDamageEvent>(ApplyDamage_ControlMagic, priority: EventPriorities.VeryHigh - 1000);
        }

        private void ApplyDamage_Dice(EntityUid effectEnt, EffectBaseDamageDiceComponent effDice, BeforeApplyEffectDamageEvent args)
        {
            if (args.Cancelled)
                return;

            if (!_newEffects.TryGetEffectDice(args.Source, args.OutInnerTarget, effectEnt, args.CommonArgs.Power, args.CommonArgs.SkillLevel, args.CommonArgs.MaxRange, out var dice, out var formulaArgs, args.SourceCoords, args.TargetCoords, effDice))
            {
                // Should never happen.
                Logger.ErrorS("effect.damage", $"No dice found for effect {effectEnt}");
                return;
            }

            var baseDamage = dice.Roll(_rand);

            args.OutElementalPower = (int)_formulas.Calculate(effDice.ElementPower, formulaArgs, 0f);

            // >>>>>>>> elona122/shade2/proc.hsp:1689 	if rapidMagic : efP=efP/2+1:dice1=dice1/2+1:dice2 ...
            // TODO move
            if (TryComp<EffectDamageCastByRapidMagicComponent>(effectEnt, out var rapidMagic) && rapidMagic.TotalAttackCount > 1)
            {
                args.CommonArgs.Power = args.CommonArgs.Power / 2 + 1;
                formulaArgs["power"] = args.CommonArgs.Power;
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1689 	if rapidMagic : efP=efP/2+1:dice1=dice1/2+1:dice2 ...

            formulaArgs["baseDamage"] = baseDamage;

            args.OutDamage = (int)_formulas.Calculate(effDice.FinalDamage, formulaArgs, baseDamage);
        }

        private void ApplyDamage_Retarget(EntityUid uid, EffectDamageRetargetComponent component, BeforeApplyEffectDamageEvent args)
        {
            if (args.Cancelled || !IsAlive(args.OutInnerTarget))
                return;

            foreach (var criteria in component.Rules)
            {
                switch (criteria)
                {
                    default:
                        break;
                    case EffectRetargetRule.AlwaysRider:
                        if (_mounts.TryGetRider(args.OutInnerTarget.Value, out var rider))
                            args.OutInnerTarget = rider.Owner;
                        break;
                    case EffectRetargetRule.AlwaysMount:
                        if (_mounts.TryGetMount(args.OutInnerTarget.Value, out var mount))
                            args.OutInnerTarget = mount.Owner;
                        break;
                }
            }
        }

        private void ApplyDamage_CastInstead(EntityUid uid, EffectDamageCastInsteadComponent component, BeforeApplyEffectDamageEvent args)
        {
            if (args.Cancelled)
                return;

            bool Matches(EntityUid uid, CastInsteadCriteria criteria)
            {
                switch (criteria)
                {
                    case CastInsteadCriteria.Any:
                        return true;
                    case CastInsteadCriteria.Player:
                        return _gameSession.IsPlayer(uid);
                    case CastInsteadCriteria.PlayerOrAlly:
                        return _parties.IsInPlayerParty(uid);
                    case CastInsteadCriteria.NotPlayer:
                        return !_gameSession.IsPlayer(uid);
                    case CastInsteadCriteria.Other:
                        return !_parties.IsInPlayerParty(uid);
                }
                return false;
            }

            if (Matches(args.Source, component.IfSource) && (!IsAlive(args.OutInnerTarget) || Matches(args.OutInnerTarget.Value, component.IfTarget)))
            {
                if (component.EffectID != null)
                {
                    var result = _newEffects.Apply(args.Source, args.OutInnerTarget, args.TargetCoords, component.EffectID.Value, args.Args);
                    args.Cancel();
                    args.TurnResult = result;
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                    args.OutDidSomething = true;
                    args.Cancel();
                }
                return;
            }
        }

        /// <summary>
        /// Filter effect targets by relation.
        /// </summary>
        private void ApplyDamage_Relations(EntityUid uid, EffectDamageRelationsComponent component, BeforeApplyEffectDamageEvent args)
        {
            if (args.Cancelled)
                return;

            // Null check instead of liveness
            // (entity might be dead and player could revive them)
            if (args.OutInnerTarget == null)
            {
                args.Cancel();
                return;
            }

            var relation = _factions.GetRelationTowards(args.Source, args.OutInnerTarget.Value);
            if (!component.ValidRelations.Includes(relation))
            {
                args.Cancel();
                return;
            }
        }

        private void ApplyDamage_SuccessRate(EntityUid uid, EffectDamageSuccessRateComponent component, BeforeApplyEffectDamageEvent args)
        {
            if (args.Cancelled)
                return;

            if (component.MessageKey != null)
                _mes.Display(Loc.GetString(component.MessageKey.Value, ("source", args.Source), ("target", args.OutInnerTarget)), entity: args.OutInnerTarget);

            var formulaArgs = _newEffects.GetEffectDamageFormulaArgs(uid, args);
            var rate = (float)_formulas.Calculate(component.SuccessRate, formulaArgs, 1.0);

            if (!_rand.Prob(rate))
            {
                args.Cancel();
                return;
            }
        }

        private enum ControlMagicStatus
        {
            Success,
            Partial,
            Failure
        }
        private record class ControlMagicResult(ControlMagicStatus Status, int NewDamage);

        private ControlMagicResult ProcControlMagic(EntityUid source, EntityUid target, int damage)
        {
            if (!_skills.TryGetKnown(source, Protos.Skill.ControlMagic, out var controlMagicLv)
                || !_parties.IsInSameParty(source, target))
            {
                return new(ControlMagicStatus.Failure, damage);
            }

            if (controlMagicLv.Level.Buffed * 5 > _rand.Next(damage + 1))
            {
                damage = 0;
            }
            else
            {
                damage = _rand.Next(damage * 100 / (100 + controlMagicLv.Level.Buffed * 10) + 1);
            }

            return new(damage <= 0 ? ControlMagicStatus.Success : ControlMagicStatus.Partial, damage);
        }

        private void ApplyDamage_ControlMagic(EntityUid uid, EffectDamageControlMagicComponent component, BeforeApplyEffectDamageEvent args)
        {
            if (args.Cancelled || !IsAlive(args.OutInnerTarget))
                return;

            var result = ProcControlMagic(args.Source, args.OutInnerTarget.Value, args.OutDamage);
            args.OutDamage = result.NewDamage;

            switch (result.Status)
            {
                case ControlMagicStatus.Success:
                    _mes.Display(Loc.GetString("Elona.Magic.ControlMagic.PassesThrough", ("source", args.Source), ("target", args.OutInnerTarget)), entity: args.OutInnerTarget);
                    args.TurnResult = TurnResult.Failed;
                    args.Cancel();
                    break;
                case ControlMagicStatus.Partial:
                    _skills.GainSkillExp(args.Source, Protos.Skill.ControlMagic, 30, 2);
                    break;
                case ControlMagicStatus.Failure:
                default:
                    break;
            }
        }
    }
}
