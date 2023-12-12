using OpenNefia.Content.Damage;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.GameController;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;

/*
 * TODO I think status effects should be entities. There are already a lot of prototype callbacks.
 */

namespace OpenNefia.Content.StatusEffects
{
    public sealed partial class VanillaStatusEffectsSystem : EntitySystem
    {
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IVanillaEnchantmentsSystem _vanillaEncs = default!;

        private void AdjustPowerFromQuality(P_StatusEffectCalcAdjustedPowerEvent ev, int i, int j)
        {
            var quality = CompOrNull<QualityComponent>(ev.Entity)?.Quality.Buffed ?? Quality.Bad;
            if (quality > Quality.Good && !_rand.OneIn(_levels.GetLevel(ev.Entity) / i + 1))
            {
                ev.OutPower = 0;
                return;
            }

            ev.OutPower /= j;
        }

        #region Elona.Sick

        public void Sick_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            ev.OutPower /= 10;
        }

        public void Sick_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 10 + 1;
        }

        public void Sick_OnTurnEnd(StatusEffectPrototype proto, P_StatusEffectOnTurnEndEvent ev)
        {
            if (_rand.OneIn(80))
            {
                var attb = _skills.PickRandomBaseAttribute();
                if (!_vanillaEncs.HasSustainEnchantment(ev.Entity, attb.GetStrongID()))
                {
                    var delta = -_skills.BaseLevel(ev.Entity, attb) / 25 + 1;
                    // TODO stat adjustments
                    _refresh.Refresh(ev.Entity);
                }
            }
            if (_rand.OneIn(5) && TryComp<SkillsComponent>(ev.Entity, out var skills))
            {
                skills.CanRegenerateThisTurn = false;
            }
            if (!_parties.IsInPlayerParty(ev.Entity))
            {
                var quality = CompOrNull<QualityComponent>(ev.Entity)?.Quality.Buffed ?? Quality.Bad;
                if (quality >= Quality.Great && _rand.OneIn(200))
                {
                    _statusEffects.HealFully(ev.Entity, Protos.StatusEffect.Sick);
                }
            }
        }

        #endregion

        #region Elona.Dimming

        public void Dimming_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            AdjustPowerFromQuality(ev, 3, 8);
        }

        public void Dimming_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 3 + 1;
        }

        public void Dimming_BeforeTurnStart(StatusEffectPrototype proto, P_StatusEffectBeforeTurnStartEvent ev)
        {
            if (_statusEffects.GetTurns(ev.Entity, Protos.StatusEffect.Dimming) > 60)
            {
                if (_gameSession.IsPlayer(ev.Entity))
                    _gameController.Wait(0.18f);
                ev.Handle(TurnResult.Failed);
            }
        }

        #endregion

        #region Elona.Bleeding

        public void Bleeding_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            var quality = CompOrNull<QualityComponent>(ev.Entity)?.Quality.Buffed ?? Quality.Bad;

            ev.OutPower /= 25;
            if (quality > Quality.Good)
                ev.OutPower /= 2;
        }

        public void Bleeding_OnTurnEnd(StatusEffectPrototype proto, P_StatusEffectOnTurnEndEvent ev)
        {
            var turns = _statusEffects.GetTurns(ev.Entity, Protos.StatusEffect.Bleeding);
            var hp = CompOrNull<SkillsComponent>(ev.Entity)?.HP ?? 1;
            _damage.DamageHP(ev.Entity, _rand.Next(hp * (1 + turns / 4) / 100 + 3) + 1, damageType: new GenericDamageType("Elona.DamageType.Bleeding"));

            if (CompOrNull<CuresBleedingQuicklyComponent>(ev.Entity)?.CuresBleedingQuickly.Buffed ?? false)
                _statusEffects.Heal(ev.Entity, Protos.StatusEffect.Bleeding, 3);

            if (TryComp<SkillsComponent>(ev.Entity, out var skills))
                skills.CanRegenerateThisTurn = false;
        }

        #endregion

        #region Elona.Drunk

        public void Drunk_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            ev.OutPower /= 10;
        }

        #endregion

        #region Elona.Insanity

        public void Insanity_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            ev.OutPower /= 8;
        }

        public void Insanity_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 3 + 1;
        }

        public void Insanity_OnTurnEnd(StatusEffectPrototype proto, P_StatusEffectOnTurnEndEvent ev)
        {
            if (_rand.OneIn(3))
                _mes.Display(Loc.GetPrototypeString(Protos.StatusEffect.Insanity, "Dialog", ("entity", ev.Entity)), entity: ev.Entity);

            if (_rand.OneIn(5))
                _statusEffects.AddTurns(ev.Entity, Protos.StatusEffect.Confusion, _rand.Next(10));
            if (_rand.OneIn(5))
                _statusEffects.AddTurns(ev.Entity, Protos.StatusEffect.Dimming, _rand.Next(10));
            if (_rand.OneIn(5))
                _statusEffects.AddTurns(ev.Entity, Protos.StatusEffect.Sleep, _rand.Next(10));
            if (_rand.OneIn(5))
                _statusEffects.AddTurns(ev.Entity, Protos.StatusEffect.Fear, _rand.Next(10));
        }

        #endregion

        #region Elona.Fear

        public void Fear_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            AdjustPowerFromQuality(ev, 5, 7);
        }

        #endregion

        #region Elona.Choking

        public void Choking_BeforeTurnStart(StatusEffectPrototype proto, P_StatusEffectBeforeTurnStartEvent ev)
        {
            if (_gameSession.IsPlayer(ev.Entity))
                _gameController.Wait(0.18f);
            ev.Handle(TurnResult.Failed);
        }

        public void Choking_OnTurnEnd(StatusEffectPrototype proto, P_StatusEffectOnTurnEndEvent ev)
        {
            if (_statusEffects.GetTurns(ev.Entity, Protos.StatusEffect.Choking) % 3 == 0)
            {
                _mes.Display(Loc.GetPrototypeString(Protos.StatusEffect.Choking, "Dialog"), entity: ev.Entity);
            }

            _statusEffects.AddTurns(ev.Entity, Protos.StatusEffect.Choking, 1);

            if (_statusEffects.GetTurns(ev.Entity, Protos.StatusEffect.Choking) > 15)
                _damage.DamageHP(ev.Entity, 500, damageType: new GenericDamageType("Elona.DamageType.Choking"));

            if (TryComp<SkillsComponent>(ev.Entity, out var skills))
                skills.CanRegenerateThisTurn = false;
        }

        #endregion

        #region Elona.Poison

        public void Poison_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            AdjustPowerFromQuality(ev, 3, 5);
        }

        public void Poison_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 3 + 3;
        }

        public void Poison_OnTurnEnd(StatusEffectPrototype proto, P_StatusEffectOnTurnEndEvent ev)
        {
            if (TryComp<SkillsComponent>(ev.Entity, out var skills))
                skills.CanRegenerateThisTurn = false;
            _damage.DamageHP(ev.Entity, _rand.Next(2 + _skills.Level(ev.Entity, Protos.Skill.AttrConstitution) / 10), damageType: new GenericDamageType("Elona.DamageType.Poison"));
        }

        #endregion

        #region Elona.Confusion

        public void Confusion_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            AdjustPowerFromQuality(ev, 2, 7);
        }

        public void Confusion_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 3 + 1;
        }

        #endregion

        #region Elona.Sleep

        public void Sleep_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            AdjustPowerFromQuality(ev, 5, 4);
        }

        public void Sleep_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 3 + 1;
        }

        public void Sleep_BeforeTurnStart(StatusEffectPrototype proto, P_StatusEffectBeforeTurnStartEvent ev)
        {
            if (_gameSession.IsPlayer(ev.Entity))
                _gameController.Wait(0.06f);
            ev.Handle(TurnResult.Failed);
        }

        public void Sleep_OnTurnEnd(StatusEffectPrototype proto, P_StatusEffectOnTurnEndEvent ev)
        {
            _damage.HealHP(ev.Entity, 1, showMessage: false);
            _damage.HealMP(ev.Entity, 1, showMessage: false);
        }

        #endregion

        #region Elona.Paralysis

        public void Paralysis_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            AdjustPowerFromQuality(ev, 1, 10);
        }

        public void Paralysis_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 3 + 1;
        }

        public void Paralysis_BeforeTurnStart(StatusEffectPrototype proto, P_StatusEffectBeforeTurnStartEvent ev)
        {
            if (_gameSession.IsPlayer(ev.Entity))
                _gameController.Wait(0.06f);
            ev.Handle(TurnResult.Failed);
        }

        #endregion

        #region Elona.Blindness

        public void Blindness_CalcAdjustedPower(StatusEffectPrototype proto, P_StatusEffectCalcAdjustedPowerEvent ev)
        {
            AdjustPowerFromQuality(ev, 2, 6);
        }

        public void Blindness_CalcAdditivePower(StatusEffectPrototype proto, P_StatusEffectCalcAdditivePowerEvent ev)
        {
            ev.OutPower = ev.OutPower / 3 + 1;
        }

        public void Blindness_BeforeTurnStart(StatusEffectPrototype proto, P_StatusEffectBeforeTurnStartEvent ev)
        {
            if (TryComp<VisibilityComponent>(ev.Entity, out var vis))
                vis.FieldOfViewRadius.Buffed = 2;
        }

        public void Blindness_OnAdd(StatusEffectPrototype proto, P_StatusEffectOnAddEvent ev)
        {
            if (TryComp<VisibilityComponent>(ev.Entity, out var vis))
                vis.FieldOfViewRadius.Buffed = 2;
        }

        public void Blindness_OnRemove(StatusEffectPrototype proto, P_StatusEffectOnRemoveEvent ev)
        {
            if (TryComp<VisibilityComponent>(ev.Entity, out var vis))
                vis.FieldOfViewRadius.Reset();
        }

        #endregion
    }

    [PrototypeEvent(typeof(StatusEffectPrototype))]
    public sealed class P_StatusEffectOnAddEvent : PrototypeEventArgs
    {
        public EntityUid Entity { get; }

        public P_StatusEffectOnAddEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }

    [PrototypeEvent(typeof(StatusEffectPrototype))]
    public sealed class P_StatusEffectOnRemoveEvent : PrototypeEventArgs
    {
        public EntityUid Entity { get; }

        public P_StatusEffectOnRemoveEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }

    [PrototypeEvent(typeof(StatusEffectPrototype))]
    public sealed class P_StatusEffectCalcAdditivePowerEvent : PrototypeEventArgs
    {
        public EntityUid Entity { get; }
        public int OriginalPower { get; set; }

        public int OutPower { get; set; }

        public P_StatusEffectCalcAdditivePowerEvent(EntityUid entity, int power)
        {
            Entity = entity;
            OriginalPower = power;
            OutPower = power;
        }
    }

    [PrototypeEvent(typeof(StatusEffectPrototype))]
    public sealed class P_StatusEffectCalcAdjustedPowerEvent : PrototypeEventArgs
    {
        public EntityUid Entity { get; }
        public int OriginalPower { get; set; }

        public int OutPower { get; set; }

        public P_StatusEffectCalcAdjustedPowerEvent(EntityUid entity, int power)
        {
            Entity = entity;
            OriginalPower = power;
            OutPower = power;
        }
    }

    [PrototypeEvent(typeof(StatusEffectPrototype))]
    public sealed class P_StatusEffectBeforeTurnStartEvent : TurnResultPrototypeEventArgs
    {
        public EntityUid Entity { get; }

        public P_StatusEffectBeforeTurnStartEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }

    [PrototypeEvent(typeof(StatusEffectPrototype))]
    public sealed class P_StatusEffectOnTurnEndEvent : PrototypeEventArgs
    {
        public EntityUid Entity { get; }

        public P_StatusEffectOnTurnEndEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }
}