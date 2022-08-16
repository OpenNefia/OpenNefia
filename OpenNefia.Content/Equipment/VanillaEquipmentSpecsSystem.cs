using OpenNefia.Content.Combat;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Weight;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Equipment
{
    public sealed class VanillaEquipmentSpecsSystem : EntitySystem
    {
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;

        public const int MaxItemGenerationTries = 15;

        public EntityUid? DefaultGenerateEquipment(P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            ev.TemplateEntry.ItemFilter.MinLevel = _randomGen.CalcObjectLevel(ev.Chara);
            ev.TemplateEntry.ItemFilter.Quality = _randomGen.CalcObjectQuality(ev.TemplateEntry.ItemFilter.Quality ?? Qualities.Quality.Bad);

            var item = _itemGen.GenerateItem(ev.CharaEquipSlots.Container, ev.TemplateEntry.ItemFilter);
            return item;
        }

        public void DefaultOnGenerateEquipment(EquipmentSpecPrototype proto, P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            if (ev.Handled)
                return;

            EntityUid? item = DefaultGenerateEquipment(ev);

            if (IsAlive(item))
                ev.Handle(item.Value);
        }

        public void GenerateLightWeapon(EquipmentSpecPrototype proto, P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            for (var i = 0; i < MaxItemGenerationTries; i++)
            {
                var item = DefaultGenerateEquipment(ev);
                if (IsAlive(item))
                {
                    var weight = CompOrNull<WeightComponent>(item.Value)?.Weight ?? 0;
                    if (weight > WeaponWeight.Light && i < MaxItemGenerationTries - 1)
                    {
                        EntityManager.DeleteEntity(item.Value);
                    }
                    else
                    {
                        ev.Handle(item.Value);
                        return;
                    }
                }
            }
        }

        public void GenerateHeavyWeapon(EquipmentSpecPrototype proto, P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            for (var i = 0; i < MaxItemGenerationTries; i++)
            {
                var item = DefaultGenerateEquipment(ev);
                if (IsAlive(item))
                {
                    var weight = CompOrNull<WeightComponent>(item.Value)?.Weight ?? 0;
                    if (weight < WeaponWeight.Heavy && i < MaxItemGenerationTries - 1)
                    {
                        EntityManager.DeleteEntity(item.Value);
                    }
                    else
                    {
                        ev.Handle(item.Value);
                        return;
                    }
                }
            }
        }
    }

    [PrototypeEvent(typeof(EquipmentSpecPrototype))]
    public sealed class P_EquipmentSpecOnGenerateEquipmentEvent : HandledPrototypeEventArgs
    {
        public EntityUid Chara => CharaEquipSlots.Owner;
        public InventoryComponent CharaEquipSlots { get; }
        public EquipSlotInstance EquipSlot { get; }
        public EquipmentTemplateEntry TemplateEntry { get; }

        public EntityUid? OutItem { get; set; } = null;
        public ISet<PrototypeId<EquipmentSpecPrototype>> OutSpecBlacklist { get; } = new HashSet<PrototypeId<EquipmentSpecPrototype>>();

        public void Handle(EntityUid item)
        {
            Handled = true;
            OutItem = item;
        }

        public P_EquipmentSpecOnGenerateEquipmentEvent(InventoryComponent charaInventory, EquipSlotInstance equipSlot, EquipmentTemplateEntry entry)
        {
            CharaEquipSlots = charaInventory;
            EquipSlot = equipSlot;
            TemplateEntry = entry;
        }
    }
}