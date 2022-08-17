using OpenNefia.Content.Combat;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Roles;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System.Collections.Generic;

namespace OpenNefia.Content.Equipment
{
    public interface IEquipmentGenSystem : IEntitySystem
    {
        void ApplyEquipmentTemplate(EntityUid chara, EquipmentTemplate template, EquipSlotsComponent? equipSlots = null, InventoryComponent? inv = null);
        void GenerateAndEquipEquipment(EntityUid chara, InventoryComponent? inv = null);
        EquipmentTemplate GenerateEquipmentTemplate(EntityUid chara);
        void GenerateEquipment(EntityUid chara, EquipSlotsComponent? equipSlots = null, InventoryComponent? inv = null);
    }

    public sealed class EquipmentGenSystem : EntitySystem, IEquipmentGenSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEquipmentSystem _equipment = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<EquipmentGenComponent, EntityGenerateEquipmentEvent>(AddInitialEquipment, priority: EventPriorities.VeryHigh);
            SubscribeComponent<GenTwoHandedWeaponComponent, EntityGenerateEquipmentEvent>(UpgradePrimaryToTwoHanded, priority: EventPriorities.VeryHigh + 1000);
        }

        private void AddInitialEquipment(EntityUid uid, EquipmentGenComponent component, EntityGenerateEquipmentEvent args)
        {
            foreach (var (specID, entry) in component.InitialEquipment)
            {
                if (entry.OneIn != null && !_rand.OneIn(entry.OneIn.Value))
                    continue;
                else if (entry.Prob != null && _rand.Prob(entry.Prob.Value))
                    continue;

                args.OutEquipTemplate.Entries[specID] = entry;
            }
        }

        private void UpgradePrimaryToTwoHanded(EntityUid uid, GenTwoHandedWeaponComponent component, EntityGenerateEquipmentEvent args)
        {
            if (args.OutEquipTemplate.Entries.TryGetValue(Protos.EquipmentSpec.PrimaryWeapon, out var primary))
            {
                args.OutEquipTemplate.Entries[Protos.EquipmentSpec.TwoHandedWeapon] = primary;
                args.OutEquipTemplate.Entries.Remove(Protos.EquipmentSpec.PrimaryWeapon);
            }
        }

        public void GenerateAndEquipEquipment(EntityUid chara, InventoryComponent? inv = null)
        {
            if (!Resolve(chara, ref inv))
                return;

            var hasWeapon = false;

            for (var i = 0; i < 100; i++)
            {
                var deleteCandidates = _inv.EnumerateItems(chara, inv)
                    .Where(i => !HasComp<AlwaysDropOnDeathComponent>(i))
                    .ToList();

                if (deleteCandidates.Any())
                {
                    var toDelete = _rand.PickAndTake(deleteCandidates);
                    EntityManager.DeleteEntity(toDelete);
                }

                var filter = new ItemFilter()
                {
                    MinLevel = _levels.GetLevel(chara)
                };

                if (HasComp<RoleAdventurerComponent>(chara))
                {
                    filter.Quality = Qualities.Quality.Good;
                }
                else
                {
                    filter.Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal);
                }

                PrototypeId<TagPrototype>? category = null;

                foreach (var slot in _equipSlots.GetEquipSlots(chara))
                {
                    if (!_equipSlots.TryGetContainerForEquipSlot(chara, slot, out var container))
                        continue;

                    if (IsAlive(container.ContainedEntity))
                    {
                        if (slot.ID == Protos.EquipSlot.Hand && !hasWeapon && _tags.HasTag(container.ContainedEntity.Value, Protos.Tag.ItemCatEquipMelee))
                        {
                            hasWeapon = true;
                        }
                    }
                    else
                    {
                        if (slot.ID == Protos.EquipSlot.Hand && !hasWeapon)
                        {
                            category = Protos.Tag.ItemCatEquipMelee;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Head)
                        {
                            category = Protos.Tag.ItemCatEquipHead;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Body)
                        {
                            category = Protos.Tag.ItemCatEquipBody;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Ranged)
                        {
                            category = Protos.Tag.ItemCatEquipRanged;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Ammo)
                        {
                            category = Protos.Tag.ItemCatEquipAmmo;
                            break;
                        }
                    }
                }

                if (category == null)
                    break;

                filter.Tags = new[] { category.Value };
                var item = _itemGen.GenerateItem(inv.Container, filter);
                if (!IsAlive(item))
                    return;

                EnsureComp<IdentifyComponent>(item.Value).IdentifyState = IdentifyState.Full;

                var quality = CompOrNull<QualityComponent>(item.Value)?.Quality.Base ?? Quality.Bad;

                if (quality >= Quality.Great && HasComp<EquipmentComponent>(item.Value) && HasComp<RoleAdventurerComponent>(chara))
                {
                    // TODO news
                }

                _equipment.EquipIfHigherValueInSlotForNPC(chara, item.Value);

                if (!HasComp<RoleAdventurerComponent>(chara) && !_rand.OneIn(3))
                    break;
            }
        }

        public EquipmentTemplate GenerateEquipmentTemplate(EntityUid chara)
        {
            var quality = CompOrNull<QualityComponent>(chara)?.Quality.Buffed ?? Quality.Bad;

            float itemGenProb;
            int addQuality = 0;

            if (quality <= Quality.Normal)
            {
                itemGenProb = 0.3f;
            }
            else if (quality == Quality.Good)
            {
                itemGenProb = 0.6f;
            }
            else if (quality == Quality.Great)
            {
                itemGenProb = 0.8f;
                addQuality = 1;
            }
            else
            {
                itemGenProb = 1f;
                addQuality = 1;
            }

            var template = new EquipmentTemplate(itemGenProb);

            if (TryComp<EquipmentGenComponent>(chara, out var equip) && equip.EquipmentType != null)
            {
                var pev = new P_EquipmentTypeOnInitializeEquipmentEvent(chara, template);
                _protos.EventBus.RaiseEvent(equip.EquipmentType.Value, pev);
                template = pev.OutEquipTemplate;
            }

            if (quality >= Quality.Great && template.Entries.Count > 0)
            {
                var i = 0;
                while (i < 2)
                {
                    if (_rand.OneIn(2))
                    {
                        var entry = _rand.Pick(template.Entries.Values);
                        entry.ItemFilter.Quality = Quality.Good;
                    }
                    if (_rand.OneIn(2))
                    {
                        i++;
                    }
                }
            }

            var ev = new EntityGenerateEquipmentEvent(template);
            RaiseEvent(chara, ev);
            template = ev.OutEquipTemplate;

            foreach (var specifier in template.Entries.Values)
            {
                specifier.ItemFilter.Quality ??= Quality.Bad;
                specifier.ItemFilter.Quality = EnumHelpers.Clamp((Quality)((int)(specifier.ItemFilter.Quality ?? Quality.Bad) + addQuality), Quality.Bad, Quality.God);
            }

            return template;
        }

        public void ApplyEquipmentTemplate(EntityUid chara, EquipmentTemplate template, EquipSlotsComponent? equipSlots = null, InventoryComponent? inv = null)
        {
            if (!Resolve(chara, ref equipSlots) || !Resolve(chara, ref inv))
                return;

            Dictionary<PrototypeId<EquipmentSpecPrototype>, int?> slotsLeftPerEntry = new();
            foreach (var (specID, entry) in template.Entries)
            {
                slotsLeftPerEntry[specID] = entry.SlotsToApplyTo ?? _protos.Index(specID).MaxEquipSlotsToApplyTo;
            }

            // Specifiers should be applied in prototype order, because ordering
            // is important for the blacklist system to work.
            // 
            // Example: If PrimaryWeapon is specified in the EquipmentType and
            // TwoHandedWeapon is specified in InitialEquipment, then
            // TwoHandedWeapon should override PrimaryWeapon by removing it from
            // the list, to avoid generating two primary weapons.
            //
            // In the HSP version you would set the flag "eqTwoWield" and
            // PrimaryWeapon would be treated like TwoHandedWeapon instead.
            var protoComp = _protos.GetComparator<EquipmentSpecPrototype>();
            var entries = new OrderedDictionary<PrototypeId<EquipmentSpecPrototype>, EquipmentTemplateEntry>();
            entries.AddRange(template.Entries);
            entries.Sort((a, b) => protoComp.Compare(a.Key, b.Key));

            foreach (var slot in _equipSlots.GetEquipSlots(chara, equipSlots))
            {
                if (!_equipSlots.TryGetContainerForEquipSlot(chara, slot, out var container))
                    continue;

                if (IsAlive(container.ContainedEntity))
                    continue;

                // Let's see if there's a way to randomly generate equipment for
                // this body part type. This should always be true for the
                // vanilla body parts.
                foreach (var (specID, entry) in template.Entries)
                {
                    var specProto = _protos.Index(specID);

                    if (!slotsLeftPerEntry.TryGetValue(specID, out var left))
                    {
                        continue;
                    }

                    if (specProto.ValidEquipSlots.Contains(slot.ID))
                    {
                        // Grab the generator for this spec kind and run it. For
                        // example, Elona.MultiWeapon will generate enough
                        // weapons to fill all the character's hand slots, while
                        // Elona.PrimaryWeapon stops generating after the first
                        // hand slot was found.
                        var pev = new P_EquipmentSpecOnGenerateEquipmentEvent(inv, slot, entry);
                        _protos.EventBus.RaiseEvent(specProto, pev);
                        var item = pev.OutItem;

                        if (left != null)
                        {
                            if (left.Value <= 1)
                            {
                                slotsLeftPerEntry.Remove(specID);
                            }
                            else
                            {
                                slotsLeftPerEntry[specID] = left.Value - 1;
                            }
                        }

                        foreach (var blacklistedID in specProto.BlacklistedSpecs)
                            slotsLeftPerEntry.Remove(blacklistedID);

                        if (IsAlive(item))
                        {
                            if (!_equipSlots.TryEquip(chara, item.Value, slot, force: true, equipSlots: equipSlots))
                            {
                                Logger.ErrorS("equipment.gen", $"Could not equip generated equipment for {chara}, slot {slot.ID}, item {item}");
                                EntityManager.DeleteEntity(item.Value);
                            }
                            break;
                        }
                        else
                        {
                            Logger.ErrorS("equipment.gen", $"No generated equipment for {chara}, slot {slot.ID}");
                        }
                    }
                }
            }
        }

        public void GenerateEquipment(EntityUid chara, EquipSlotsComponent? equipSlots = null, InventoryComponent? inv = null)
        {
            if (!Resolve(chara, ref equipSlots) || !Resolve(chara, ref inv))
                return;

            var template = GenerateEquipmentTemplate(chara);
            ApplyEquipmentTemplate(chara, template, equipSlots, inv);
        }
    }

    [DataDefinition]
    public class EquipmentTemplateEntry
    {
        public EquipmentTemplateEntry() { }

        public EquipmentTemplateEntry(ItemFilter itemFilter, int? slotsToApplyTo = null)
        {
            ItemFilter = itemFilter;
            SlotsToApplyTo = slotsToApplyTo;
        }

        [DataField(required: true)]
        public ItemFilter ItemFilter { get; } = new();

        [DataField]
        public int? SlotsToApplyTo { get; }
    }


    [DataDefinition]
    public class InitialEquipmentEntry : EquipmentTemplateEntry
    {
        public InitialEquipmentEntry() { }

        [DataField]
        public int? OneIn { get; }

        [DataField]
        public float? Prob { get; }
    }

    /// <summary>
    /// Describes how to randomly generate equipment for a character.
    /// </summary>
    [DataDefinition]
    public sealed class EquipmentTemplate
    {
        public EquipmentTemplate(float itemGenProb)
        {
            ItemGenProb = itemGenProb;
        }

        [DataField]
        public Dictionary<PrototypeId<EquipmentSpecPrototype>, EquipmentTemplateEntry> Entries { get; } = new();

        /// <summary>
        /// Probability for generating items. Used by equipment type prototype
        /// events.
        /// </summary>
        [DataField]
        public float ItemGenProb { get; }
    }

    public sealed class EntityGenerateEquipmentEvent : EntityEventArgs
    {
        public EquipmentTemplate OutEquipTemplate { get; }

        public EntityGenerateEquipmentEvent(EquipmentTemplate template)
        {
            OutEquipTemplate = template;
        }
    }
}