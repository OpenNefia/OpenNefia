using OpenNefia.Content.Audio;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Equipment
{
    [RegisterComponent]
    public class EquipmentComponent : Component
    {
        public override string Name => "Equipment";

        [DataField("equipSlots", required: true)]
        private HashSet<PrototypeId<EquipSlotPrototype>> _equipSlots = new();

        public IReadOnlySet<PrototypeId<EquipSlotPrototype>> EquipSlots => _equipSlots;

        [DataField]
        public SoundSpecifier? EquipSound { get; set; } = default!;
    }
}
