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
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Enchantments
{
    public interface IEnchantmentSystem : IEntitySystem
    {
        EntityUid? SpawnEnchantment(IContainer container, PrototypeId<EntityPrototype> encID, EntityUid item, ref int power, int cursePower = 0, bool randomize = true, string source = EnchantmentSources.Generated);
        
        EntityUid? AddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, int cursePower = 0, bool randomize = true, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null);
        EntityUid? AddEnchantment(EntityUid item, EntityUid enchantment, int power, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null);

        bool TryAddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, [NotNullWhen(true)] out EntityUid? enchantment, int cursePower = 0, bool randomize = true, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null);
        bool TryAddEnchantment(EntityUid item, EntityUid enchantment, int power, [NotNullWhen(true)] out EntityUid? mergedEnchantment, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null, EnchantmentComponent? enc = null);

        void AddEnchantmentFromSpecifier(EntityUid uid, EnchantmentSpecifer spec, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null);

        bool TryAddEnchantmentFromSpecifier(EntityUid uid, EnchantmentSpecifer spec, [NotNullWhen(true)] out EntityUid? enchantment, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null);
        
        string GetEnchantmentDescription(EntityUid enchantment, EntityUid item, EntityUid? wielder = null, int? adjustedPower = null, bool noPowerText = false, EnchantmentComponent? encComp = null);

        bool TryFindMergeableEnchantment(EntityUid item, EnchantmentComponent enchantment, [NotNullWhen(true)] out EnchantmentComponent? mergeable, EnchantmentsComponent? itemEncs = null);

        void RemoveEnchantmentsWithSource(EntityUid item, string source, EnchantmentsComponent? encs = null);

        int CalcEnchantmentAdjustedPower(EntityUid enchantment, EntityUid item, EnchantmentComponent? enc = null);
        IEnumerable<EnchantmentComponent> EnumerateEnchantments(EntityUid item, EnchantmentsComponent? encs = null);

        IEnumerable<T> QueryEnchantmentsOnItem<T>(EntityUid item, EnchantmentsComponent? encs = null) where T : class, IComponent;
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
        [Dependency] private readonly IEquipmentSystem _equipment = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IItemDescriptionSystem _itemDescriptions = default!;
        [Dependency] private readonly IContainerSystem _containers = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;

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

        public void AddEnchantmentFromSpecifier(EntityUid uid, EnchantmentSpecifer spec, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null)
            => TryAddEnchantmentFromSpecifier(uid, spec, out _, source, encs);

        public bool TryAddEnchantmentFromSpecifier(EntityUid uid, EnchantmentSpecifer spec, [NotNullWhen(true)] out EntityUid? enchantment, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null)
        {
            if (!Resolve(uid, ref encs))
            {
                enchantment = null;
                return false;
            }

            if (!TryAddEnchantment(uid, spec.ProtoID, spec.Power, out enchantment, cursePower: spec.CursePower, randomize: spec.Randomize, source: source, encs: encs))
            {
                return false;
            }

            foreach (var (name, comp) in spec.Components)
            {
                AddComponent(enchantment.Value, _componentFactory, name, comp, null);
            }

            return true;
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
            if (args.Handled || !TryComp<TurnOrderComponent>(equipper, out var turnOrder))
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

            var wielder = _gameSession.Player;
            if (_containers.TryGetContainingContainer(item, out var container))
                wielder = container.Owner;

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

        public string GetEnchantmentDescription(EntityUid enchantment, EntityUid item, EntityUid? wielder = null, int? adjustedPower = null, bool noPowerText = false, EnchantmentComponent? encComp = null)
        {
            if (!Resolve(enchantment, ref encComp))
                return "???";

            if (wielder == null)
            {
                wielder = _gameSession.Player;
                if (_containers.TryGetContainingContainer(item, out var container))
                    wielder = container.Owner;
            }
            adjustedPower ??= CalcEnchantmentAdjustedPower(enchantment, item, encComp);

            string? desc = string.Empty;
            if (TryProtoID(encComp.Owner, out var encID) && Loc.TryGetPrototypeString(encID.Value, "Enchantment.Description", out var encDesc))
                desc = encDesc;

            var hasProvidedDesc = !string.IsNullOrEmpty(desc);

            var ev = new GetEnchantmentDescriptionEventArgs(adjustedPower.Value, item, wielder.Value, desc);
            RaiseEvent(encComp.Owner, ev);
            desc = ev.OutDescription;

            if (TryProtoID(encComp.Owner, out var protoID) && Loc.TryGetPrototypeString(protoID.Value, "Enchantment.Description", out var desc2, ("item", item), ("adjustedPower", adjustedPower.Value), ("wielder", wielder)))
                desc = desc2;

            if ((ev.OutShowPower ?? hasProvidedDesc) && !noPowerText)
                desc += " " + GetEnchantmentPowerText(ev.OutGrade);
            
            return desc;
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
                case EnchantmentAlignmentType.Positive:
                    return EnchantmentAlignment.Positive;
                case EnchantmentAlignmentType.Negative:
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

        public EntityUid? SpawnEnchantment(IContainer container, PrototypeId<EntityPrototype> encID, EntityUid item, ref int power, int cursePower = 0, bool randomize = true, string source = EnchantmentSources.Generated)
        {
            var encArgs = new EnchantmentGenArgs(item, power, cursePower, source, randomize);
            var args = EntityGenArgSet.Make(encArgs);
            var ent = _entityGen.SpawnEntity(encID, container, args: args);

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

            var enc = EnsureComp<EnchantmentComponent>(ent.Value);
            enc.PowerContributions.Add(new EnchantmentPowerContrib(power, source));
            enc.TotalPower += power;

            return ent;
        }

        public EntityUid? AddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, int cursePower = 0, bool randomize = true, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null)
        {
            if (!Resolve(item, ref encs))
                return null;

            var ent = SpawnEnchantment(encs.Container, encID, item, ref power, cursePower, randomize, source);

            if (!IsAlive(ent) || !TryAddEnchantmentInternal(item, ent.Value, power, out var realEnc, source, encs))
                return null;

            return realEnc;
        }

        public EntityUid? AddEnchantment(EntityUid item, EntityUid enchantment, int power, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null)
        {
            TryAddEnchantment(item, enchantment, power, out var mergedEnchantment, source, encs);
            return mergedEnchantment;
        }

        public bool TryAddEnchantment(EntityUid item, PrototypeId<EntityPrototype> encID, int power, [NotNullWhen(true)] out EntityUid? enchantment, int cursePower = 0, bool randomize = true, string source = EnchantmentSources.Generated, EnchantmentsComponent? encs = null)
        {
            enchantment = AddEnchantment(item, encID, power, cursePower, randomize, source, encs);
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
                mergeable.TotalPower += power;
                mergedEnchantment = mergeable.Owner;
                EntityManager.DeleteEntity(enchantment);
                return true;
            }

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

        public void RemoveEnchantmentsWithSource(EntityUid item, string source, EnchantmentsComponent? encs = null)
        {
            foreach (var enc in EnumerateEnchantments(item, encs).ToList())
            {
                foreach (var power in enc.PowerContributions.Where(p => p.Source == source).ToList())
                {
                    enc.TotalPower -= power.Power;
                    enc.PowerContributions.Remove(power);
                }

                if (enc.PowerContributions.Count == 0)
                    EntityManager.DeleteEntity(enc.Owner);
            }
        }

        public IEnumerable<T> QueryEnchantmentsOnItem<T>(EntityUid item, EnchantmentsComponent? encs = null) where T : class, IComponent
        {
            foreach (var enc in EnumerateEnchantments(item, encs))
            {
                if (TryComp<T>(enc.Owner, out var encComp))
                    yield return encComp;
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