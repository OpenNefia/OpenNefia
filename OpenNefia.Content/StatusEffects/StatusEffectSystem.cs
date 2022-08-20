using OpenNefia.Content.Activity;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Hud;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.StatusEffects
{
    public interface IStatusEffectSystem : IEntitySystem
    {
        /// <summary>
        /// Checks if this entity has the indicated status effect.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="statusEffects"></param>
        /// <returns></returns>
        bool HasEffect(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null);
        IEnumerable<KeyValuePair<PrototypeId<StatusEffectPrototype>, StatusEffect>> EnumerateStatusEffects(EntityUid entity, StatusEffectsComponent? statusEffects = null);
        bool CanApplyTo(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null);

        void Remove(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null);
        void RemoveAll(EntityUid entity, StatusEffectsComponent? statusEffects = null);
        int GetTurns(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null);
        bool SetTurns(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int turns, bool force = false, StatusEffectsComponent? statusEffects = null);
        bool AddTurns(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int turns, bool force = false, StatusEffectsComponent? statusEffects = null);

        bool Apply(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int power = 10, bool showMessage = true, StatusEffectsComponent? statusEffects = null);
        bool Heal(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int turns, bool showMessage = true, StatusEffectsComponent? statusEffects = null);
        bool HealFully(EntityUid entity, PrototypeId<StatusEffectPrototype> id, bool showMessage = true, StatusEffectsComponent? statusEffects = null);
    }

    public sealed class StatusEffectSystem : EntitySystem, IStatusEffectSystem
    {
        [Dependency] private readonly ISlotSystem _slots = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IEmotionIconSystem _emotionIcons = default!;

        public override void Initialize()
        {
            SubscribeComponent<StatusEffectsComponent, GetStatusIndicatorsEvent>(AddStatusIndicators, priority: EventPriorities.VeryHigh);
            SubscribeComponent<StatusEffectsComponent, OnCharaSleepEvent>(HandleCharaSleep);
            SubscribeComponent<StatusEffectsComponent, EntityTurnStartingEventArgs>(HandleTurnStarting, priority: EventPriorities.High);
            SubscribeComponent<StatusEffectsComponent, EntityPassTurnEventArgs>(HandlePassTurn, priority: EventPriorities.High);
            SubscribeComponent<StatusEffectsComponent, EntityTurnEndingEventArgs>(HandleTurnEnding, priority: EventPriorities.High);
        }

        private void AddStatusIndicators(EntityUid uid, StatusEffectsComponent effects, GetStatusIndicatorsEvent ev)
        {
            foreach (var proto in _protos.EnumeratePrototypes<StatusEffectPrototype>())
            {
                var id = proto.GetStrongID();
                if (HasEffect(uid, id, effects))
                {
                    var turns = GetTurns(uid, id, effects);
                    for (var i = proto.Indicators.Count - 1; i >= 0; i--)
                    {
                        var turnThreshold = proto.Indicators[i];
                        if (turns >= turnThreshold)
                        {
                            ev.OutIndicators.Add(new StatusIndicator()
                            {
                                Text = Loc.GetPrototypeString(id, $"Indicator.{i}"),
                                Color = proto.Color
                            });
                            break;
                        }
                    }
                }
            }
        }

        private void HandleCharaSleep(EntityUid uid, StatusEffectsComponent component, OnCharaSleepEvent args)
        {
            foreach (var (id, effect) in EnumerateStatusEffects(uid).ToList())
            {
                var proto = _protos.Index(id);
                if (proto.RemoveOnSleep)
                {
                    Remove(uid, id);
                }
            }
        }

        private void HandleTurnStarting(EntityUid uid, StatusEffectsComponent component, EntityTurnStartingEventArgs args)
        {
            // TODO OnTurnStart
            if (args.Handled)
                return;

            foreach (var (id, effect) in EnumerateStatusEffects(uid).ToList())
            {
                var pev = new P_StatusEffectBeforeTurnStartEvent(uid);
                _protos.EventBus.RaiseEvent(id, pev);
                if (pev.Handled)
                {
                    args.Handle(pev.TurnResult);
                    return;
                }
            }
        }

        private void HandlePassTurn(EntityUid uid, StatusEffectsComponent component, EntityPassTurnEventArgs args)
        {
            if (args.Handled)
                return;

            foreach (var (id, effect) in EnumerateStatusEffects(uid).ToList())
            {
                var proto = _protos.Index(id);
                if (proto.EmotionIconId != null)
                {
                    _emotionIcons.SetEmotionIcon(uid, proto.EmotionIconId);
                }

                var pev = new P_StatusEffectOnPassTurnEvent(uid);
                _protos.EventBus.RaiseEvent(proto, pev);
                if (pev.Handled)
                {
                    args.Handle(pev.TurnResult);
                    return;
                }
            }
        }

        private void HandleTurnEnding(EntityUid uid, StatusEffectsComponent component, EntityTurnEndingEventArgs args)
        {
            if (args.Handled)
                return;

            foreach (var (id, effect) in EnumerateStatusEffects(uid).ToList())
            {
                var proto = _protos.Index(id);
                if (proto.AutoHeal)
                {
                    Heal(uid, id, 1);
                }
            }
        }

        public int GetTurns(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return 0;

            if (!statusEffects.StatusEffects.TryGetValue(id, out var effect))
                return 0;

            return effect.TurnsRemaining;
        }

        public bool HasEffect(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null)
        {
            return GetTurns(entity, id, statusEffects) > 0;
        }

        public IEnumerable<KeyValuePair<PrototypeId<StatusEffectPrototype>, StatusEffect>> EnumerateStatusEffects(EntityUid entity, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return Enumerable.Empty<KeyValuePair<PrototypeId<StatusEffectPrototype>, StatusEffect>>();

            return statusEffects.StatusEffects;
        }

        public bool CanApplyTo(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return false;

            return !statusEffects.StatusEffectImmunities.Contains(id);
        }

        public bool SetTurns(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int turns, bool force = false, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return false;

            if (IsImmuneToEffect(entity, id, statusEffects) && !force)
            {
                statusEffects.StatusEffects.Remove(id);
                return false;
            }

            var hadEffect = HasEffect(entity, id, statusEffects);
            if (!hadEffect && turns <= 0)
                return false;

            var eff = statusEffects.StatusEffects.GetValueOrInsert(id, () => AddNewStatusEffect(entity, id));
            eff.TurnsRemaining = turns;
            if (turns <= 0)
            {
                if (hadEffect)
                {
                    var pev = new P_StatusEffectOnRemoveEvent(entity);
                    _protos.EventBus.RaiseEvent(id, pev);
                }
                _slots.RemoveSlot(entity, eff.Slot);
                statusEffects.StatusEffects.Remove(id);
            }
            else
            {
                if (!hadEffect)
                {
                    var pev = new P_StatusEffectOnAddEvent(entity);
                    _protos.EventBus.RaiseEvent(id, pev);
                }
            }

            return true;
        }

        public bool AddTurns(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int turns, bool force = false, StatusEffectsComponent? statusEffects = null)
        {
            var oldTurns = GetTurns(entity, id, statusEffects);
            return SetTurns(entity, id, Math.Max(oldTurns + turns, 0), force, statusEffects);
        }

        private StatusEffect AddNewStatusEffect(EntityUid entity, PrototypeId<StatusEffectPrototype> id)
        {
            var proto = _protos.Index(id);
            var slot = _slots.AddSlot(entity, proto.Components, overwrite: true);
            return new StatusEffect()
            {
                Slot = slot
            };
        }

        private bool IsImmuneToEffect(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return false;

            return statusEffects.StatusEffectImmunities.Contains(id);
        }

        public void Remove(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null)
        {
            SetTurns(entity, id, 0, force: false, statusEffects);
        }

        public void RemoveAll(EntityUid entity, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return;

            foreach (var effId in statusEffects.StatusEffects.Keys)
            {
                Remove(entity, effId, statusEffects);
            }
        }

        public bool Apply(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int power = 10, bool showMessage = true, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return false;

            if (power <= 10)
                return false;

            var proto = _protos.Index(id);

            var turns = CalcStatusEffectTurns(entity, power, proto);

            if (turns <= 0)
                return false;

            var currentTurns = GetTurns(entity, id, statusEffects);
            bool success;

            if (currentTurns <= 0)
            {
                success = SetTurns(entity, id, turns, force: false, statusEffects);
                if (success && showMessage && Loc.TryGetPrototypeString(id, "Apply", out var text, ("entity", entity)))
                {
                    _mes.Display(text, UiColors.MesPurple, entity: entity);
                }
            }
            else
            {
                var additive = CalcAdditivePower(entity, id, turns);
                turns = currentTurns + additive;
                success = SetTurns(entity, id, turns, force: false, statusEffects);
            }

            if (proto.StopsActivity)
                _activities.RemoveActivity(entity);

            return success;
        }

        public bool Heal(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int turns, bool showMessage = true, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(entity, ref statusEffects))
                return false;

            if (turns <= 0)
                return false;

            var currentTurns = GetTurns(entity, id, statusEffects);
            var newTurns = Math.Clamp(currentTurns - turns, 0, currentTurns);

            var success = SetTurns(entity, id, newTurns, force: false, statusEffects);

            if (success && showMessage && currentTurns > 0 && newTurns <= 0)
            {
                if (success && showMessage && Loc.TryGetPrototypeString(id, "Heal", out var text, ("entity", entity)))
                {
                    _mes.Display(text, entity: entity);
                }
            }

            return success;
        }

        public bool HealFully(EntityUid entity, PrototypeId<StatusEffectPrototype> id, bool showMessage = true, StatusEffectsComponent? statusEffects = null)
        {

            return Heal(entity, id, GetTurns(entity, id, statusEffects), showMessage, statusEffects);
        }

        private int CalcAdditivePower(EntityUid entity, PrototypeId<StatusEffectPrototype> id, int turns)
        {
            var pev = new P_StatusEffectCalcAdditivePowerEvent(entity, turns);
            _protos.EventBus.RaiseEvent(id, pev);
            return pev.OutPower;
        }

        private int CalcStatusEffectTurns(EntityUid entity, int power, StatusEffectPrototype proto)
        {
            if (proto.RelatedElement != null)
            {
                var elementProto = _protos.Index(proto.RelatedElement.Value);
                var resistGrade = _resists.Grade(entity, elementProto);
                if (resistGrade >= ResistGrades.Minimum && power < 40)
                    return 0;

                power = (_rand.Next(power / 2 + 1) + (power / 2)) * 100 / (50 + resistGrade * 50);
            }

            var pev = new P_StatusEffectCalcAdjustedPowerEvent(entity, power);
            _protos.EventBus.RaiseEvent(proto, pev);
            return pev.OutPower;
        }
    }
}
