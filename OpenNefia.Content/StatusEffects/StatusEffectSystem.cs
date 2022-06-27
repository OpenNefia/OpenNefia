using OpenNefia.Content.Activity;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Resists;
using OpenNefia.Content.UI;
using OpenNefia.Content.World;
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
        bool HasEffect(EntityUid entity, PrototypeId<StatusEffectPrototype> id, StatusEffectsComponent? statusEffects = null);
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
                    // TODO on_remove
                }
                _slots.RemoveSlot(entity, eff.Slot);
                statusEffects.StatusEffects.Remove(id);
            }
            else
            {
                if (!hadEffect)
                {
                    // TODO on_add
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

            // TODO before_apply
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
            // TODO
            return turns / 3 + 1;
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

            // TODO calc_adjusted_power

            return power;
        }
    }
}
