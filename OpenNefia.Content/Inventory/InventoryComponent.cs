using OpenNefia.Content.Equipment;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Equipment.EquipSlotPrototype>;
using System;

namespace OpenNefia.Content.Inventory
{
    /// <summary>
    /// Contains a character's items and slots for equipment.
    /// </summary>
    [RegisterComponent]
    public class InventoryComponent : Component, ISerializationHooks
    {
        public static readonly ContainerId ContainerIdInventory = new("Elona.Inventory");

        /// <inheritdoc />
        public override string Name => "Inventory";

        public Container Container { get; private set; } = default!;

        /// <summary>
        /// Valid equipment slots on this entity. These all have active containers in the
        /// entity's container manager.
        /// **Do NOT update this manually!** Please go through <see cref="InventorySystem"/> instead.
        /// </summary>
        /// <remarks>
        /// Contrast with Robust's <c>InventoryTemplatePrototype</c>. This needs to be mutable
        /// because you can add and remove body parts during the game.
        /// </remarks>
        [DataField]
        public List<EquipSlotInstance> EquipSlots { get; } = new();

        [DataField]
        public int? MaxWeight { get; set; }

        protected override void Initialize()
        {
            base.Initialize();

            Container = ContainerHelpers.EnsureContainer<Container>(Owner, ContainerIdInventory);
        }

        bool ISerializationHooks.AfterCompare(object? other)
        {
            if (other is not InventoryComponent otherInv)
                return false;

            // Don't stack if either inventory is full.
            if (Container.ContainedEntities.Count >= 0 || otherInv.Container.ContainedEntities.Count > 0)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Active instance of a container originating from an <see cref="EquipSlotPrototype"/>.
    /// This is necessary since, in Elona, you can have more than one body part of the same type,
    /// and you can also add and remove body parts at runtime.
    /// </summary>
    /// <remarks>
    /// Contrast with Robust's <c>SlotDefinition.</c>
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
