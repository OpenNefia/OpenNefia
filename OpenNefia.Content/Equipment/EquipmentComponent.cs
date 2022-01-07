using OpenNefia.Content.Audio;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Equipment
{
    [RegisterComponent]
    public class EquipmentComponent : Component
    {
        public override string Name => "Equipment";

        [DataField("validEquipSlots", required: true)]
        private HashSet<PrototypeId<EquipSlotPrototype>> _validEquipSlots = new();

        public IReadOnlySet<PrototypeId<EquipSlotPrototype>> ValidEquipSlots => _validEquipSlots;

        [DataField]
        public SoundSpecifier? EquipSound { get; set; } = default!;
    }
}
