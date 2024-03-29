﻿using OpenNefia.Content.Logic;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System.Text;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Log;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Utility;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Content.Food;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Items;
using OpenNefia.Content.Identify;
using OpenNefia.Content.UI;
using OpenNefia.Core.Containers;
using OpenNefia.Content.Combat;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.Game;
using OpenNefia.Content.Charas;

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

        IEnumerable<(EnchantmentComponent, T)> QueryEnchantmentsOnItem<T>(EntityUid item, EnchantmentsComponent? encs = null) where T : class, IComponent;
        IEnumerable<(EnchantmentComponent, T)> QueryEnchantmentsOnEquipper<T>(EntityUid equipper, EquipSlotsComponent? equipSlots = null) where T : class, IComponent;
        int GetEnchantmentPower<T>(EntityUid item, EnchantmentsComponent? encs = null) where T : class, IComponent;
        int GetTotalEquippedEnchantmentPower<T>(EntityUid equipper, EquipSlotsComponent? equipSlots = null) where T : class, IComponent;
        bool HasEnchantmentEquipped<T>(EntityUid equipper)
          where T : class, IComponent;
    }

    public sealed partial class EnchantmentSystem : EntitySystem, IEnchantmentSystem
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
        [Dependency] private readonly IEntityFactory _entityFactory = default!;


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

            _entityFactory.UpdateEntityComponents(enchantment.Value, spec.Components);

            return true;
        }

        private EntityUid GetItemWielder(EntityUid item)
        {
            if (_containers.TryGetContainingContainer(item, out var container) && HasComp<CharaComponent>(container.Owner))
                return container.Owner;
            else
                return _gameSession.Player;
        }

        public string GetEnchantmentDescription(EntityUid enchantment, EntityUid item, EntityUid? wielder = null, int? adjustedPower = null, bool noPowerText = false, EnchantmentComponent? encComp = null)
        {
            if (!Resolve(enchantment, ref encComp))
                return "???";

            wielder ??= GetItemWielder(item);

            adjustedPower ??= CalcEnchantmentAdjustedPower(enchantment, item, encComp);

            string? desc = string.Empty;
            if (TryProtoID(encComp.Owner, out var encID) && Loc.TryGetPrototypeString(encID.Value, "Enchantment.Description", out var encDesc))
                desc = encDesc;

            var hasProvidedDesc = !string.IsNullOrEmpty(desc);

            var ev = new GetEnchantmentDescriptionEventArgs(adjustedPower.Value, item, wielder.Value, desc);
            RaiseEvent(encComp.Owner, ev);
            desc = ev.OutDescription;

            if (TryProtoID(encComp.Owner, out var protoID) && Loc.TryGetPrototypeString(protoID.Value, "Enchantment.Description", out var desc2, ("item", item), ("wielder", wielder), ("adjustedPower", adjustedPower.Value)))
                desc = desc2;

            if ((ev.OutShowPower ?? hasProvidedDesc) && !noPowerText)
                desc += " " + GetEnchantmentPowerText(ev.OutGrade);
            
            return desc;
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

        public IEnumerable<(EnchantmentComponent, T)> QueryEnchantmentsOnItem<T>(EntityUid item, EnchantmentsComponent? encs = null) where T : class, IComponent
        {
            foreach (var enc in EnumerateEnchantments(item, encs))
            {
                if (TryComp<T>(enc.Owner, out var encComp))
                    yield return (enc, encComp);
            }
        }

        public IEnumerable<(EnchantmentComponent, T)> QueryEnchantmentsOnEquipper<T>(EntityUid equipper, EquipSlotsComponent? equipSlots = null) where T : class, IComponent
        {
            foreach (var item in _equipSlots.EnumerateEquippedEntities(equipper, equipSlots))
            {
                foreach (var pair in QueryEnchantmentsOnItem<T>(item))
                    yield return pair;
            }
        }

        public int GetEnchantmentPower<T>(EntityUid item, EnchantmentsComponent? encs = null) where T : class, IComponent
        {
            return QueryEnchantmentsOnItem<T>(item, encs)
                .Sum(pair => pair.Item1.TotalPower);
        }

        public int GetTotalEquippedEnchantmentPower<T>(EntityUid equipper, EquipSlotsComponent? equipSlots = null) where T : class, IComponent
        {
            return _equipSlots.EnumerateEquippedEntities(equipper, equipSlots)
                .Sum(item => GetEnchantmentPower<T>(item));
        }

        bool IEnchantmentSystem.HasEnchantmentEquipped<T>(EntityUid equipper)
        {
            return QueryEnchantmentsOnEquipper<T>(equipper).Any();
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