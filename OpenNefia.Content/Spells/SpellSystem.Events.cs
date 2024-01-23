using OpenNefia.Content.Buffs;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Effects.New.Unique;
using OpenNefia.Core.Formulae;

namespace OpenNefia.Content.Spells
{
    public sealed partial class SpellSystem
    {
        [Dependency] private readonly IFormulaEngine _formulas = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectApplyBuffsComponent, GetEffectDescriptionEvent>(GetSpellDesc_Buff, priority: EventPriorities.VeryLow - 10000);
            SubscribeEntity<GetEffectDescriptionEvent>(GetSpellDesc_Default, priority: EventPriorities.VeryLow);
            SubscribeEntity<BeforeSpellEffectInvokedEvent>(BeforeSpellEffect_ProcVanillaCastingEvents);
            SubscribeBroadcast<GetBuffDescriptionEvent>(GetBuffDesc_Default, priority: EventPriorities.VeryLow);
        }

        private void GetSpellDesc_Buff(EntityUid effectUid, EffectApplyBuffsComponent component, GetEffectDescriptionEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:2209 	s=""	 ...
            if (args.Handled)
                return;

            if (component.Buffs.Count != 1)
                return;

            var buffID = component.Buffs[0].ID;

            var adjusted = _buffs.CalcBuffPowerAndTurns(buffID, args.Power);

            var buffProto = _protos.Index(buffID);
            var ev = new GetBuffDescriptionEvent(buffProto, args.Power, adjusted.Power);
            RaiseEvent(ev);
            args.OutDescription = Loc.GetString("Elona.Spells.Description.TurnCounter", ("turns", adjusted.Turns))
                + Loc.Space + ev.OutDescription;
            args.Handled = true;
            // <<<<<<<< elona122/shade2/command.hsp:2215 	} ...
        }

        private void GetBuffDesc_Default(GetBuffDescriptionEvent args)
        {
            if (args.Handled)
                return;

            args.OutDescription = Loc.GetPrototypeString(args.BuffPrototype, "Buff.Description", ("power", args.Power));
            args.Handled = true;
        }

        private void GetSpellDesc_Default(EntityUid effectUid, GetEffectDescriptionEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:2217 	calcSkill i,cc,calcSpellPower(i,cc) ...
            if (args.Handled)
                return;

            if (!_newEffects.TryGetEffectDice(args.Caster, null, effectUid, args.Power, args.SkillLevel, args.MaxRange, out var dice, out _))
                return;

            if (dice.X > 0)
            {
                args.OutDescription += Loc.Space + dice.ToString();
            }
            else if (dice.Bonus > 0)
            {
                args.OutDescription += Loc.Space + Loc.GetString("Elona.Spells.Description.Power", ("power", dice.Bonus));
            }
            // <<<<<<<< elona122/shade2/command.hsp:2231 		} ...
        }

        private void BeforeSpellEffect_ProcVanillaCastingEvents(EntityUid effect, BeforeSpellEffectInvokedEvent args)
        {
            if (args.Handled)
                return;

            if (_statusEffects.HasEffect(args.Caster, Protos.StatusEffect.Confusion)
                || _statusEffects.HasEffect(args.Caster, Protos.StatusEffect.Dimming))
            {
                _mes.Display(Loc.GetString("Elona.Spells.Cast.Confused", ("caster", args.Caster)), entity: args.Caster);
                if (!_spellbooks.TryToReadSpellbook(args.Caster, args.Spell.Difficulty, Level(args.Caster, args.Spell)))
                {
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }

            var spells = Comp<SpellsComponent>(args.Caster);
            LocaleKey castingStyle;
            if (spells.CastingStyle != null)
            {
                var key = new LocaleKey($"Elona.Spells.CastingStyle").With(spells.CastingStyle.Value);
                if (Loc.KeyExists(key))
                    castingStyle = key;
                else
                    castingStyle = "Elona.Spells.CastingStyle.Default";
            }
            else
            {
                castingStyle = "Elona.Spells.CastingStyle.Default";
            }

            if (_gameSession.IsPlayer(args.Caster))
            {
                var skillName = Loc.GetPrototypeString(args.Spell.SkillID, "Name");
                _mes.Display(Loc.GetString(castingStyle.With("WithSkillName"), ("caster", args.Caster), ("target", args.Target), ("skillName", skillName)), entity: args.Caster);
            }
            else
            {
                _mes.Display(Loc.GetString(castingStyle.With("Generic"), ("caster", args.Caster), ("target", args.Target)), entity: args.Caster);
            }

            if (_buffs.HasBuff<BuffMistOfSilenceComponent>(args.Caster))
            {
                _mes.Display(Loc.GetString("Elona.Spells.Cast.Silenced", ("caster", args.Caster)), entity: args.Caster);
                args.Handle(TurnResult.Failed);
                return;
            }

            if (!_rand.Prob(CalcSpellSuccessRate(args.Spell, args.Caster, effect)))
            {
                if (_vis.IsInWindowFov(args.Caster))
                {
                    _mes.Display(Loc.GetString("Elona.Spells.Cast.Fail", ("caster", args.Caster)), entity: args.Caster);
                    var anim = new SpellCastFailureMapDrawable();
                    _mapDrawables.Enqueue(anim, args.Caster);
                }
                args.Handle(TurnResult.Failed);
                return;
            }

            var encPower = _enchantments.GetTotalEquippedEnchantmentPower<EncEnhanceSpellsComponent>(args.Caster);
            if (encPower > 0)
            {
                args.Args.Power = (int)(args.Args.Power * (100f + encPower / 10f) / 100f);
            }
        }
    }
}