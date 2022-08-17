using OpenNefia.Core.Areas;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.EquipSlots.EquipSlotPrototype>;

namespace OpenNefia.Content.EquipSlots
{
    /// <summary>
    /// Holds entity equipment information.
    /// </summary>
    [RegisterComponent]
    public class EquipSlotsComponent : Component
    {
        public override string Name => "EquipSlots";

        [DataField("initialEquipSlots")]
        private List<EquipSlotPrototypeId> _initialEquipSlots = new();

        /// <summary>
        /// Equipment slots to generate this character with.
        /// </summary>
        public IReadOnlyList<EquipSlotPrototypeId> InitialEquipSlots => _initialEquipSlots;

        /// <summary>
        /// Valid equipment slots on this entity. These all have active containers in the
        /// entity's container manager.
        /// **Do NOT update this manually!** Please go through <see cref="EquipSlotsSystem"/> instead.
        /// </summary>
        /// <remarks>
        /// Contrast with SS14's <c>InventoryTemplatePrototype</c>. This needs to be mutable
        /// because you can add and remove body parts during the game.
        /// </remarks>
        [DataField]
        public List<EquipSlotInstance> EquipSlots { get; } = new();
    }

    /// <summary>
    /// Active instance of a container originating from an <see cref="EquipSlotPrototype"/>.
    /// This is necessary since, in Elona, you can have more than one body part of the same type,
    /// and you can also add and remove body parts at runtime.
    /// </summary>
    /// <remarks>
    /// Contrast with SS14's <c>SlotDefinition.</c>
    /// </remarks>
    [DataDefinition]
    public class EquipSlotInstance
    {
        /// <summary>
        /// ID of the equipment slot type.
        /// </summary>
        [DataField(required: true)]
        public EquipSlotPrototypeId ID { get; } = default!;

        /// <summary>
        /// ID of the container bound to the equipment slot.
        /// </summary>
        [DataField(required: true)]
        public ContainerId ContainerID { get; } = ContainerId.Invalid;

        public EquipSlotInstance()
        {
        }

        public EquipSlotInstance(EquipSlotPrototypeId id, ContainerId containerID)
        {
            ID = id;
            ContainerID = containerID;
        }

        public override string ToString()
        {
            return $"{nameof(EquipSlotInstance)}({ContainerID})";
        }
    }
}
