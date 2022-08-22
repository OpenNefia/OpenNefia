using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Log;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Utility;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Food;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Items;
using OpenNefia.Content.Identify;
using OpenNefia.Content.UI;
using OpenNefia.Core.Containers;
using OpenNefia.Content.Combat;
using NetVips;

namespace OpenNefia.Content.Enchantments
{
    public interface IEnchantmentSystem : IEntitySystem
    {
        EntityUid? AddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, string source = "Generated", int cursePower = 0, bool randomize = true, EnchantmentsComponent? encs = null);

        bool TryAddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, [NotNullWhen(true)] out EntityUid? enchantment, string source = "Generated", int cursePower = 0, bool randomize = true, EnchantmentsComponent? encs = null);
        bool TryAddEnchantment(EntityUid item, EntityUid enchantment, int power, [NotNullWhen(true)] out EntityUid? mergedEnchantment, string source = "Generated", EnchantmentsComponent? encs = null, EnchantmentComponent? enc = null);

        bool TryFindMergeableEnchantment(EntityUid item, EnchantmentComponent enchantment, [NotNullWhen(true)] out EnchantmentComponent? mergeable, EnchantmentsComponent? itemEncs = null);

        int CalcEnchantmentAdjustedPower(EntityUid enchantment, EntityUid item, EnchantmentComponent? enc = null);
        IEnumerable<EnchantmentComponent> EnumerateEnchantments(EntityUid item, EnchantmentsComponent? encs = null);
    }

    public sealed class EnchantmentSystem : EntitySystem, IEnchantmentSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;

        public override void Initialize()
        {
            SubscribeComponent<EnchantmentsComponent, EntityBeingGeneratedEvent>(Enchantments_BeingGenerated, priority: EventPriorities.High);
            SubscribeComponent<EnchantmentsComponent, EntityRefreshEvent>(Enchantments_Refresh);
            SubscribeComponent<EnchantmentsComponent, ApplyEquipmentToEquipperEvent>(Enchantments_ApplyEquipment, priority: EventPriorities.VeryLow);
            SubscribeComponent<EnchantmentsComponent, AfterApplyFoodEffectsEvent>(Enchantments_ApplyFoodEffects, priority: EventPriorities.VeryHigh);
            SubscribeComponent<EnchantmentsComponent, GetItemDescriptionEventArgs>(Enchantments_GetItemDescription, priority: EventPriorities.High);
            SubscribeEntity<AfterPhysicalAttackHitEventArgs>(Enchantments_AfterPhysicalAttackHit, priority: EventPriorities.High);
        }

        private void Enchantments_BeingGenerated(EntityUid uid, EnchantmentsComponent encs, ref EntityBeingGeneratedEvent args)
        {
            foreach (var spec in encs.InitialEnchantments)
            {
                if (TryAddEnchantment(uid, spec.ProtoID, spec.Power, out var enc, source: EnchantmentSources.EntityPrototype, cursePower: spec.CursePower, randomize: spec.Randomize, encs: encs))
                {
                    foreach (var (name, comp) in spec.Components)
                    {
                        AddComponent(enc.Value, _componentFactory, name, comp, null);
                    }
                }
            }
        }

        // TODO dedup from EntityFactory
        private void AddComponent(EntityUid entity, IComponentFactory factory, string compName,
            IComponent data, ISerializationContext? context)
        {
            var compType = factory.GetRegistration(compName).Type;

            if (!EntityManager.TryGetComponent(entity, compType, out var component))
            {
                var newComponent = (Component)factory.GetComponent(compName);
                newComponent.Owner = entity;
                EntityManager.AddComponent(entity, newComponent);
                component = newComponent;
            }

            // TODO use this value to support struct components
            _ = _serialization.Copy(data, component, context);
        }

        private void Enchantments_Refresh(EntityUid item, EnchantmentsComponent encs, ref EntityRefreshEvent args)
        {
            if (!TryComp<ValueComponent>(item, out var value))
                return;

            foreach (var enc in EnumerateEnchantments(item, encs))
            {
                value.Value.Buffed = (int)(value.Value.Buffed * enc.ValueModifier * enc.SubEnchantmentCount);
            }
        }

        private void Enchantments_ApplyEquipment(EntityUid item, EnchantmentsComponent encs, ref ApplyEquipmentToEquipperEvent args)
        {
            foreach (var enc in encs.Container.ContainedEntities)
            {
                var adjustedPower = CalcEnchantmentAdjustedPower(enc, item);
                var ev = new ApplyEnchantmentEffectsEvent(adjustedPower, args.Equipper, item);
                RaiseEvent(enc, ref ev);
            }
        }

        private void Enchantments_GettingInserted(EntityUid uid, EnchantmentsComponent component, ContainerIsInsertingAttemptEvent args)
        {
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

        private void Enchantments_ApplyFoodEffects(EntityUid item, EnchantmentsComponent component, AfterApplyFoodEffectsEvent args)
        {
            foreach (var enc in EnumerateEnchantments(item, component))
            {
                var adjustedPower = CalcEnchantmentAdjustedPower(enc.Owner, item, enc);
                var ev = new ApplyEnchantmentFoodEffectsEvent(adjustedPower, args.Eater, item);
                RaiseEvent(enc.Owner, ref ev);
            }
        }

        private void Enchantments_GetItemDescription(EntityUid item, EnchantmentsComponent encs, GetItemDescriptionEventArgs args)
        {
            if (_identify.GetIdentifyState(item) < IdentifyState.Full)
                return;

            foreach (var enc in EnumerateEnchantments(item, encs))
            {
                var adjustedPower = CalcEnchantmentAdjustedPower(enc.Owner, item, enc);

                var desc = EntityManager.GetComponents(enc.Owner)
                    .WhereAssignable<IComponent, IEnchantmentComponent>()
                    .Select(ec => ec.Description)
                    .WhereNotNull()
                    .FirstOrDefault()
                    ?? string.Empty;

                var ev = new GetEnchantmentDescriptionEventArgs(adjustedPower, item, desc);
                RaiseEvent(enc.Owner, ev);
                desc = ev.OutDescription;

                if (TryProtoID(enc.Owner, out var protoID) && Loc.TryGetPrototypeString(protoID.Value, "Enchantment.Description", out var desc2, ("enchantment", enc.Owner), ("item", item), ("adjustedPower", adjustedPower)))
                    desc = desc2;

                if (ev.OutShowPower)
                    desc += " " + GetEnchantmentPowerText(ev.OutGrade);

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
            }
        }

        public static string GetEnchantmentPowerText(int grade, bool noBrackets = false)
        {
            grade = Math.Abs(grade);
            var unit = Loc.GetString("Elona.Enchantment.PowerUnit");
            var s = new StringBuilder();

            for (var i = 0; i <= grade; i++)
            {
                if (i > 4)
                {
                    s.Append("+");
                    break;
                }
                s.Append(unit);
            }

            if (!noBrackets)
            {
                s.Insert(0, "[");
                s.Append("]");
            }
            
            return s.ToString();
        }

        private EnchantmentAlignment GetEnchantmentAlignment(EnchantmentAlignmentType type, int adjustedPower)
        {
            switch (type)
            {
                case EnchantmentAlignmentType.BaseOnPower:
                default:
                    return adjustedPower >= 0 ? EnchantmentAlignment.Positive : EnchantmentAlignment.Negative;
                case EnchantmentAlignmentType.AlwaysPositive:
                    return EnchantmentAlignment.Positive;
                case EnchantmentAlignmentType.AlwaysNegative:
                    return EnchantmentAlignment.Negative;
            }
        }

        public int CalcEnchantmentAdjustedPower(EntityUid enchantment, EntityUid item, EnchantmentComponent? enc = null)
        {
            if (!Resolve(enchantment, ref enc))
                return 0;

            var ev = new CalcEnchantmentAdjustedPowerEvent(enc.TotalPower, item);
            RaiseEvent(enchantment, ref ev);
            return ev.OutPower;
        }

        public EntityUid? AddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, string source = EnchantmentSources.Generated, int cursePower = 0, bool randomize = true, EnchantmentsComponent? encs = null)
        {
            if (!Resolve(item, ref encs))
                return null;

            var encArgs = new EnchantmentGenArgs(item, power, cursePower, source, randomize);
            var args = EntityGenArgSet.Make(encArgs);
            var ent = _entityGen.SpawnEntity(encID, encs.Container, args: args);

            if (!IsAlive(ent))
                return null;

            if (!encArgs.OutIsValid)
            {
                Logger.ErrorS("enchantment", $"Enchantment {ent} ({encID}) returned not valid after generation.");

                if (IsAlive(ent.Value))
                    EntityManager.DeleteEntity(ent.Value);
                return null;
            }

            power = encArgs.OutPower;

            if (!TryAddEnchantmentInternal(item, ent.Value, power, out var realEnc, source, encs))
                return null;

            return realEnc;
        }

        public bool TryAddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, [NotNullWhen(true)] out EntityUid? enchantment, string source = EnchantmentSources.Generated, int cursePower = 0, bool randomize = true, EnchantmentsComponent? encs = null)
        {
            enchantment = AddEnchantment(item, encID, power, source, cursePower, randomize, encs);
            return IsAlive(enchantment);
        }

        public bool TryAddEnchantment(EntityUid item, EntityUid enchantment, int power, [NotNullWhen(true)] out EntityUid? mergedEnchantment, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null, EnchantmentComponent? enc = null)
        {
            if (!Resolve(item, ref encs) || !Resolve(enchantment, ref enc))
            {
                mergedEnchantment = null;
                return false;
            }
            
            if (!encs.Container.Insert(enchantment))
            {
                Logger.ErrorS("enchantment", $"Failed to insert enchantment {enchantment} into item {item}.");
                mergedEnchantment = null;
                return false;
            }

            return TryAddEnchantmentInternal(item, enchantment, power, out mergedEnchantment, source, encs, enc);
        }
         
        /// <summary>
        /// Ignores containment checks, enchantment entity is assumed to be in the enchantments
        /// container by now.
        /// </summary>
        private bool TryAddEnchantmentInternal(EntityUid item, EntityUid enchantment, int power, [NotNullWhen(true)] out EntityUid? mergedEnchantment, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null, EnchantmentComponent? enc = null)
        {
            mergedEnchantment = null;

            if (!Resolve(item, ref encs) || !Resolve(enchantment, ref enc))
                return false;

            DebugTools.Assert(encs.Container.Contains(enchantment), $"Enchantment {enchantment} not in container for {item}!");

            if (TryFindMergeableEnchantment(item, enc, out var mergeable))
            {
                Logger.DebugS("enchantment", $"Merge enchantment {enc.Owner} with {mergeable.Owner}");
                mergeable.PowerContributions.Add(new EnchantmentPowerContrib(power, source));
                mergeable.TotalPower += enc.TotalPower;
                mergedEnchantment = mergeable.Owner;
                EntityManager.DeleteEntity(enchantment);
                return true;
            }

            enc.PowerContributions.Add(new EnchantmentPowerContrib(power, source));
            enc.TotalPower += power;
            mergedEnchantment = enchantment;
            return true;
        }

        public bool TryFindMergeableEnchantment(EntityUid item, EnchantmentComponent enchantment, [NotNullWhen(true)] out EnchantmentComponent? mergeable, EnchantmentsComponent? itemEncs = null)
        {
            if (!Resolve(item, ref itemEncs))
            {
                mergeable = null;
                return false;
            }

            var comparable = EntityManager.GetComponents(enchantment.Owner)
                .WhereAssignable<IComponent, IEnchantmentComponent>()
                .ToDictionary(c => c.GetType(), c => c);
            var found = 0;

            foreach (var otherEnc in itemEncs.Container.ContainedEntities)
            {
                found = 0;

                foreach (var otherEncComp in EntityManager.GetComponents(otherEnc))
                {
                    if (otherEncComp is IEnchantmentComponent otherEncCompC)
                    {
                        var otherEncCompType = otherEncCompC.GetType();
                        if (!comparable.TryGetValue(otherEncCompType, out var ourEncComp))
                            break;

                        if (ourEncComp == otherEncComp)
                            break;

                        if (!ourEncComp.CanMergeWith(otherEncCompC))
                            break;

                        found++;
                    }
                }

                if (found == comparable.Count)
                {
                    mergeable = Comp<EnchantmentComponent>(otherEnc);
                    return true;
                }
            }

            mergeable = null;
            return false;
        }

        public IEnumerable<EnchantmentComponent> EnumerateEnchantments(EntityUid item, EnchantmentsComponent? encs = null)
        {
            if (!Resolve(item, ref encs))
                yield break;

            foreach (var enc in encs.Container.ContainedEntities)
            {
                if (TryComp<EnchantmentComponent>(enc, out var encComp))
                    yield return encComp;
                else
                    Logger.ErrorS("enchantments", $"Enchantment {enc} is missing an {nameof(EnchantmentComponent)}!");
            }
        }
    }

    public sealed class EnchantmentGenArgs : EntityGenArgs
    {
        public EnchantmentGenArgs() { }

        public EnchantmentGenArgs(EntityUid item, int power, int cursePower, string source, bool randomize)
        {
            OutPower = power;
            OutCursePower = cursePower;
            Item = item;
            Source = source;
            Randomize = randomize;
        }

        [DataField]
        public int OutPower { get; set; }

        [DataField]
        public int OutCursePower { get; set; }

        [DataField]
        public bool OutIsValid { get; set; } = true;

        [DataField]
        public EntityUid Item { get; }

        [DataField]
        public string Source { get; } = EnchantmentSources.Generated;

        [DataField]
        public bool Randomize { get; }
    }
}