using OpenNefia.Content.Inventory;
using OpenNefia.Content.RandomGen;
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

        public void DefaultOnGenerateEquipment(EquipmentSpecPrototype proto, P_EquipmentSpecOnGenerateEquipmentEvent ev)
        {
            if (ev.Handled)
                return;

            var filter = new ItemFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(ev.Chara),
                Quality = _randomGen.CalcObjectQuality(),
                Id = ev.TemplateEntry.ItemID,
                Tags = ev.TemplateEntry.Categories.ToArray(),
            };
            var item = _itemGen.GenerateItem(ev.CharaInventory.Container, filter);

            if (IsAlive(item))
                ev.Handle(item.Value);
        }

        public void MultiWeapon_OnGenerateEquipment(EquipmentSpecPrototype proto, int ev)
        {
        }
    }

    [PrototypeEvent(typeof(EquipmentSpecPrototype))]
    public sealed class P_EquipmentSpecOnGenerateEquipmentEvent : HandledPrototypeEventArgs
    {
        public EntityUid Chara => CharaInventory.Owner;
        public InventoryComponent CharaInventory { get; }
        public EquipmentTemplateEntry TemplateEntry { get; }

        public EntityUid? OutItem { get; set; } = null;

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