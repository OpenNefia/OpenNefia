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
using OpenNefia.Content.Skills;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Damage;
using NetVips;
using OpenNefia.Content.Effects.New.EffectAreas;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Combat;

namespace OpenNefia.Content.Effects.New.EffectDamage
{
    public sealed class VanillaEffectDamageSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISpatialSystem _spatials = default!;
        [Dependency] private readonly IFormulaEngine _formulaEngine = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectBaseDamageDiceComponent, ApplyEffectDamageEvent>(ApplyDamage_Dice, priority: EventPriorities.VeryHigh - 10000);
            SubscribeComponent<EffectDamageControlMagicComponent, ApplyEffectDamageEvent>(ApplyDamage_ControlMagic, priority: EventPriorities.VeryHigh - 100);

            SubscribeComponent<EffectDamageElementalComponent, ApplyEffectDamageEvent>(ApplyDamage_Elemental);
            SubscribeComponent<EffectDamageElementalComponent, GetEffectAnimationParamsEvent>(GetAnimParams_Elemental);
        }

        private IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid uid, EntityUid source, EntityUid? target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectBaseDamageDiceComponent component, EffectArgSet args)
        {
            var result = new Dictionary<string, double>();

            result["power"] = args.Power;
            result["skillLevel"] = args.SkillLevel;
            if (sourceCoords.TryDistanceFractional(EntityManager, targetCoords, out var dist))
            {
                result["distance"] = dist;
            }

            return result;
        }

        private void ApplyDamage_Dice(EntityUid uid, EffectBaseDamageDiceComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            var formulaArgs = GetEffectDamageFormulaArgs(uid, args.Source, args.InnerTarget, args.SourceCoords, args.TargetCoords, component, args.Args);

            var diceX = int.Max((int)_formulaEngine.Calculate(component.DiceX, formulaArgs, 1f), 1);
            var diceY = int.Max((int)_formulaEngine.Calculate(component.DiceY, formulaArgs, 1f), 1);
            var bonus = (int)_formulaEngine.Calculate(component.Bonus, formulaArgs, 0f);
            args.OutElementalPower = (int)_formulaEngine.Calculate(component.ElementPower, formulaArgs, 0f);

            var dice = new Dice(diceX, diceY, bonus);
            var baseDamage = dice.Roll(_rand);

            formulaArgs["baseDamage"] = baseDamage;

            args.OutDamage = (int)_formulaEngine.Calculate(component.DamageModifier, formulaArgs, baseDamage);
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

        private void ApplyDamage_ControlMagic(EntityUid uid, EffectDamageControlMagicComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            var result = ProcControlMagic(args.Source, args.InnerTarget.Value, args.OutDamage);
            args.OutDamage = result.NewDamage;

            switch (result.Status)
            {
                case ControlMagicStatus.Success:
                    _mes.Display(Loc.GetString("Elona.Magic.ControlMagic.PassesThrough", ("target", args.InnerTarget)), entity: args.InnerTarget);
                    args.Handle(TurnResult.Succeeded);
                    break;
                case ControlMagicStatus.Partial:
                    _skills.GainSkillExp(args.Source, Protos.Skill.ControlMagic, 30, 2);
                    break;
                case ControlMagicStatus.Failure:
                default:
                    break;
            }
        }

        private void GetAnimParams_Elemental(EntityUid uid, EffectDamageElementalComponent component, GetEffectAnimationParamsEvent args)
        {
            if (_protos.TryIndex(component.Element, out var eleProto))
            {
                args.OutColor = eleProto.Color;
                args.OutSound = eleProto.Sound?.GetSound();
            }
        }

        /// <summary>
        /// Shows a message like "The bolt hits the putit and" or "The bolt hits the putit.".
        /// The rest of the message is displayed in <see cref="IDamageSystem.DamageHP"/>.
        /// </summary>
        private void DisplayEffectDamageMessage(EntityUid uid, EntityUid innerTarget, DamageHPMessageTense tense)
        {
            if (TryComp<EffectDamageMessageComponent>(uid, out var damageMessage))
            {
                var endKey = tense == DamageHPMessageTense.Active ? "Other" : "Ally";
                _mes.Display(Loc.GetString(damageMessage.RootKey.With(endKey), ("entity", innerTarget)), entity: innerTarget);
            }
        }

        private void ApplyDamage_Elemental(EntityUid uid, EffectDamageElementalComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            var tense = _damages.GetDamageMessageTense(args.InnerTarget.Value);
            DisplayEffectDamageMessage(uid, args.InnerTarget.Value, tense);

            var extraArgs = new DamageHPExtraArgs()
            {
                MessageTense = tense,
                NoAttackText = true,
                // The bolt/ball/dart is the one doing the striking, not the player (directly at least)
                //
                // "You hit the putit and {transform} him..."
                //   vs.
                // "The bolt hits the putit and {transforms} him..."
                AttackerIsMessageSubject = false
            };
            var damageType = new ElementalDamageType(component.Element, args.OutElementalPower);
            _damages.DamageHP(args.InnerTarget.Value, args.OutDamage, args.Source, damageType, extraArgs);
        }
    }
}