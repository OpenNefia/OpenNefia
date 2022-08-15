using OpenNefia.Content.Combat;
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
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;

        public const int MaxItemGenerationTries = 15;

        public EntityUid? DefaultGenerateEquipment(P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            var filter = new ItemFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(ev.Chara),
                Quality = _randomGen.CalcObjectQuality(ev.TemplateEntry.Quality),
                Id = ev.TemplateEntry.ItemID,
                Tags = ev.TemplateEntry.Categories.ToArray(),
            };
            var item = _itemGen.GenerateItem(ev.CharaInventory.Container, filter);
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

        public void TwoHanded_OnGenerateEquipment(EquipmentSpecPrototype proto, P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            ev.OutSpecBlacklist.Add(Protos.EquipmentSpec.Shield);
            GenerateHeavyWeapon(proto, ev);
        }

        public void MultiWeapon_OnGenerateEquipment(EquipmentSpecPrototype proto, P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            ev.OutSpecBlacklist.Add(Protos.EquipmentSpec.PrimaryWeapon);
            GenerateLightWeapon(proto, ev);
        }
    }

    [PrototypeEvent(typeof(EquipmentSpecPrototype))]
    public sealed class P_EquipmentSpecOnGenerateEquipmentEvent : HandledPrototypeEventArgs
    {
        public EntityUid Chara => CharaInventory.Owner;
        public InventoryComponent CharaInventory { get; }
        public EquipmentTemplateEntry TemplateEntry { get; }

        public EntityUid? OutItem { get; set; } = null;
        public ISet<PrototypeId<EquipmentSpecPrototype>> OutSpecBlacklist { get; } = new HashSet<PrototypeId<EquipmentSpecPrototype>>();

        public void Handle(EntityUid item)
        {
            Handled = true;
            OutItem = item;
        }

        public P_EquipmentSpecOnGenerateEquipmentEvent(InventoryComponent charaInventory, EquipmentTemplateEntry entry)
        {
            CharaInventory = charaInventory;
            TemplateEntry = entry;
        }
    }
}