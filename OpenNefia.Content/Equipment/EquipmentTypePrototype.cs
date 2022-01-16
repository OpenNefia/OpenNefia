using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Equipment
{
    [Prototype("Elona.EquipmentType")]
    public class EquipmentTypePrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public IEquipmentTypeBehavior Behavior { get; } = default!;
    }

    public class EquipmentEntry
    {
        public string TargetPart = string.Empty;
        public string ItemCategory = string.Empty;
        public Quality ItemQuality;
    }

    public class LootEntry
    {
        public ItemFilter ItemFilter = new();
    }

    public class ItemFilter
    {
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IEquipmentTypeBehavior
    {
        void OnInitializeEquipment(EntityUid chara, List<EquipmentEntry> equipEntries, int genChance);
        void OnDropLoot(EntityUid chara, List<LootEntry> lootEntries);
    }
}
