using OpenNefia.Content.Combat;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Items;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Enchantments
{
    public sealed partial class EnchantmentSystem
    {
        public override void Initialize()
        {
            SubscribeComponent<EnchantmentsComponent, EntityBeingGeneratedEvent>(Enchantments_BeingGenerated, priority: EventPriorities.High);
            SubscribeComponent<EnchantmentsComponent, ContainerIsInsertingAttemptEvent>(Enchantments_GettingInserted, priority: EventPriorities.High);
            SubscribeComponent<EnchantmentsComponent, EntityRefreshEvent>(Enchantments_Refresh);
            SubscribeComponent<EnchantmentsComponent, ApplyEquipmentToEquipperEvent>(Enchantments_ApplyEquipment, priority: EventPriorities.VeryLow);
            SubscribeComponent<EquipSlotsComponent, EntityPassTurnEventArgs>(EquipSlots_ProcEnchantmentPassTurns, priority: EventPriorities.VeryHigh);
            SubscribeComponent<EnchantmentsComponent, AfterApplyFoodEffectsEvent>(Enchantments_ApplyFoodEffects, priority: EventPriorities.VeryHigh);
            SubscribeComponent<EnchantmentsComponent, GetItemDescriptionEventArgs>(Enchantments_GetItemDescription, priority: EventPriorities.High);
            SubscribeEntity<AfterPhysicalAttackHitEventArgs>(Enchantments_AfterPhysicalAttackHit, priority: EventPriorities.High);
        }

        private void Enchantments_BeingGenerated(EntityUid uid, EnchantmentsComponent encs, ref EntityBeingGeneratedEvent args)
        {
            foreach (var spec in encs.InitialEnchantments)
            {
                AddEnchantmentFromSpecifier(uid, spec, EnchantmentSources.EntityPrototype, encs);
            }
        }

        private void Enchantments_GettingInserted(EntityUid uid, EnchantmentsComponent component, ContainerIsInsertingAttemptEvent args)
        {
            if (args.Cancelled)
                return;

            if (args.Container.Contains(args.EntityUid))
            {
                args.Cancel();
                return;
            }

            var totalEncs = EnumerateEnchantments(args.Container.Owner, component)
                .Select(c => c.SubEnchantmentCount)
                .Sum();

            if (totalEncs >= EnchantmentsComponent.MaxEnchantments)
                args.Cancel();
        }

        private void Enchantments_Refresh(EntityUid item, EnchantmentsComponent encs, ref EntityRefreshEvent args)
        {
            if (TryComp<ValueComponent>(item, out var value))
            {
                foreach (var enc in EnumerateEnchantments(item, encs))
                {
                    value.Value.Buffed = (int)(value.Value.Buffed * enc.ValueModifier * enc.SubEnchantmentCount);
                }
            }

            // >>>>>>>> elona-next/src/mod/elona/api/Enchantment.lua:192    if object_quality < Enum.Quality.Unique then ...
            if (encs.HasRandomEnchantments)
            {
                if (TryComp<ValueComponent>(item, out value))
                {
                    value.Value.Buffed *= 3;
                }
            }
            // <<<<<<<< elona-next/src/mod/elona/api/Enchantment.lua:195    end ...
        }

        private void Enchantments_ApplyEquipment(EntityUid item, EnchantmentsComponent encs, ref ApplyEquipmentToEquipperEvent args)
        {
            foreach (var enc in EnumerateEnchantments(item, encs))
            {
                var adjustedPower = CalcEnchantmentAdjustedPower(enc.Owner, item);
                var ev = new ApplyEnchantmentOnRefreshEvent(enc.TotalPower, adjustedPower, args.Equipper, item);
                RaiseEvent(enc.Owner, ref ev);
            }
        }

        private void EquipSlots_ProcEnchantmentPassTurns(EntityUid equipper, EquipSlotsComponent component, EntityPassTurnEventArgs args)
        {
            if (args.Handled)
                return;

            foreach (var item in _equipSlots.EnumerateEquippedEntities(equipper))
            {
                foreach (var enc in EnumerateEnchantments(item))
                {
                    if (enc.TurnsPerEvent != null)
                    {
                        if (enc.TurnsUntilNextEvent < 0)
                        {
                            var adjustedPower = CalcEnchantmentAdjustedPower(enc.Owner, item, enc);
                            var ev = new ApplyEnchantmentAfterPassTurnEvent(enc.TotalPower, adjustedPower, equipper, item);
                            RaiseEvent(enc.Owner, ref ev);
                            enc.TurnsUntilNextEvent = enc.TurnsPerEvent.Value;
                        }

                        enc.TurnsUntilNextEvent--;
                    }
                }
            }
        }

        private void Enchantments_ApplyFoodEffects(EntityUid item, EnchantmentsComponent component, AfterApplyFoodEffectsEvent args)
        {
            foreach (var enc in EnumerateEnchantments(item, component))
            {
                var adjustedPower = CalcEnchantmentAdjustedPower(enc.Owner, item, enc);
                var ev = new ApplyEnchantmentFoodEffectsEvent(enc.TotalPower, adjustedPower, args.Eater, item);
                RaiseEvent(enc.Owner, ref ev);
            }
        }

        private void Enchantments_GetItemDescription(EntityUid item, EnchantmentsComponent encs, GetItemDescriptionEventArgs args)
        {
            if (_identify.GetIdentifyState(item) < IdentifyState.Full)
                return;

            var wielder = GetItemWielder(item);

            var allEncs = EnumerateEnchantments(item, encs).ToList();
            var comparer = _protos.GetComparator<EntityPrototype>();
            allEncs.Sort((a, b) => comparer.Compare(ProtoIDOrNull(a.Owner), ProtoIDOrNull(b.Owner)));

            foreach (var enc in allEncs)
            {
                var adjustedPower = CalcEnchantmentAdjustedPower(enc.Owner, item, enc);
                string desc = GetEnchantmentDescription(enc.Owner, item, wielder, adjustedPower, encComp: enc);

                var icon = enc.Icon;
                var color = enc.Color ?? UiColors.EnchantmentDefault;
                var alignment = GetEnchantmentAlignment(enc.AlignmentType, adjustedPower);

                if (alignment == EnchantmentAlignment.Negative)
                {
                    color = UiColors.EnchantmentNegative;
                    icon = ItemDescriptionIcon.Negative;
                }

                args.OutEntries.Add(new ItemDescriptionEntry()
                {
                    Text = desc,
                    Icon = icon,
                    TextColor = color,
                    IsInheritable = enc.IsInheritable
                });
            }
        }

        private void Enchantments_AfterPhysicalAttackHit(EntityUid attacker, AfterPhysicalAttackHitEventArgs args)
        {
            if (IsAlive(args.Weapon))
            {
                foreach (var enc in EnumerateEnchantments(args.Weapon.Value))
                {
                    var adjustedPower = CalcEnchantmentAdjustedPower(enc.Owner, args.Weapon.Value, enc);
                    var ev = new ApplyEnchantmentPhysicalAttackEffectsEvent(enc.TotalPower, adjustedPower, attacker, args);
                    RaiseEvent(enc.Owner, ref ev);
                }

                if (args.AttackCount == 0 && args.IsRanged && _combat.TryGetActiveAmmoEnchantment(attacker, args.Weapon.Value, out var ammoComp, out var ammoEncComp))
                {
                    var pev = new P_AmmoEnchantmentAfterRangedAttackHitEvent(attacker, args);
                    _protos.EventBus.RaiseEvent(ammoEncComp.AmmoEnchantmentID, ref pev);
                }
            }
        }
    }

    [ByRefEvent]
    public struct CalcEnchantmentAdjustedPowerEvent
    {
        public int OriginalPower { get; }
        public EntityUid Item { get; }

        public int OutPower { get; set; }

        public CalcEnchantmentAdjustedPowerEvent(int power, EntityUid item)
        {
            OriginalPower = power;
            Item = item;
            OutPower = power;
        }
    }

    public sealed class GetEnchantmentDescriptionEventArgs : EntityEventArgs
    {
        public int AdjustedPower { get; }
        public EntityUid Item { get; }
        public EntityUid ItemOwner { get; }

        public string OutDescription { get; set; }
        public int OutGrade { get; set; }
        public bool? OutShowPower { get; set; }

        public GetEnchantmentDescriptionEventArgs(int adjustedPower, EntityUid item, EntityUid equipper, string description)
        {
            AdjustedPower = adjustedPower;
            Item = item;
            ItemOwner = equipper;
            OutDescription = description;
            OutGrade = AdjustedPower;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentOnRefreshEvent
    {
        public int TotalPower { get; }
        public int AdjustedPower { get; }
        public EntityUid Equipper { get; }
        public EntityUid Item { get; }

        public ApplyEnchantmentOnRefreshEvent(int totalPower, int adjustedPower, EntityUid equipper, EntityUid item)
        {
            TotalPower = totalPower;
            AdjustedPower = adjustedPower;
            Equipper = equipper;
            Item = item;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentAfterPassTurnEvent
    {
        public int TotalPower { get; }
        public int AdjustedPower { get; }
        public EntityUid Equipper { get; }
        public EntityUid Item { get; }

        public ApplyEnchantmentAfterPassTurnEvent(int totalPower, int adjustedPower, EntityUid equipper, EntityUid item)
        {
            TotalPower = totalPower;
            AdjustedPower = adjustedPower;
            Equipper = equipper;
            Item = item;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentFoodEffectsEvent
    {
        public int TotalPower { get; }
        public int AdjustedPower { get; }
        public EntityUid Eater { get; }
        public EntityUid Item { get; }

        public ApplyEnchantmentFoodEffectsEvent(int totalPower, int adjustedPower, EntityUid eater, EntityUid item)
        {
            TotalPower = totalPower;
            AdjustedPower = adjustedPower;
            Eater = eater;
            Item = item;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentPhysicalAttackEffectsEvent
    {
        public int TotalPower { get; }
        public int AdjustedPower { get; }
        public EntityUid Attacker { get; }
        public EntityUid Weapon => PhysicalAttackArgs.Weapon!.Value;
        public EntityUid Target => PhysicalAttackArgs.Target;
        public AfterPhysicalAttackHitEventArgs PhysicalAttackArgs { get; }

        public ApplyEnchantmentPhysicalAttackEffectsEvent(int totalPower, int adjustedPower, EntityUid attacker, AfterPhysicalAttackHitEventArgs args)
        {
            TotalPower = totalPower;
            AdjustedPower = adjustedPower;
            Attacker = attacker;
            PhysicalAttackArgs = args;
        }
    }
}
