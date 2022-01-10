using OpenNefia.Core.Areas;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.EquipSlots
{
    public partial interface IEquipSlotsSystem : IEntitySystem
    {
        /// <summary>
        /// Clears and initializes equipment slots on this entity, adding an <see cref="EquipSlotsComponent"/>
        /// and a <see cref="ContainerManagerComponent"/> if needed.
        /// </summary>
        /// <param name="uid">Entity UID.</param>
        /// <param name="equipSlotProtoIds">Equip slots for this entity.</param>
        void InitializeEquipSlots(EntityUid uid, IEnumerable<PrototypeId<EquipSlotPrototype>> equipSlotProtoIds,
            EquipSlotsComponent? equipSlots = null,
            ContainerManagerComponent? containerComp = null);

        /// <summary>
        /// Tries to add a new equipment slot instance to this entity.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="slotId"></param>
        /// <param name="equipSlotInstance"></param>
        /// <param name="containerSlot"></param>
        /// <param name="equipSlotsComp"></param>
        /// <param name="containerComp"></param>
        /// <returns></returns>
        bool TryAddEquipSlot(EntityUid uid, PrototypeId<EquipSlotPrototype> slotId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance, [NotNullWhen(true)] out ContainerSlot? containerSlot,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null);

        /// <summary>
        /// Tries to to remove an existing equipment slot instance on this entity.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="instance"></param>
        /// <param name="equipSlotsComp"></param>
        /// <param name="containerComp"></param>
        /// <returns></returns>
        bool TryRemoveEquipSlot(EntityUid uid, EquipSlotInstance instance,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null);

        /// <summary>
        /// Clears all equipment slots on this entity and shuts down their containers.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="equipSlotsComp"></param>
        /// <param name="containerComp"></param>
        void ClearEquipSlots(EntityUid uid,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null);

        /// <summary>
        /// Returns true if this entity has an equip slot of the given type.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="slotId"></param>
        /// <param name="equipSlotsComp"></param>
        /// <returns></returns>
        bool HasEquipSlot(EntityUid uid, PrototypeId<EquipSlotPrototype> slotId,
            EquipSlotsComponent? equipSlotsComp = null);

        /// <summary>
        /// Returns an equipment slot with the given ID on this entity, if any.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="slotId"></param>
        /// <param name="equipSlotInstance"></param>
        /// <param name="equipSlotsComp"></param>
        /// <returns></returns>
        bool TryGetEquipSlot(EntityUid uid, PrototypeId<EquipSlotPrototype> slotId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            EquipSlotsComponent? equipSlotsComp = null);

        /// <summary>
        /// Returns true if this entity has an empty equip slot of the given type.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="slotId"></param>
        /// <param name="equipSlotsComp"></param>
        /// <returns></returns>
        bool HasEmptyEquipSlot(EntityUid uid, PrototypeId<EquipSlotPrototype> slotId,
            EquipSlotsComponent? equipSlotsComp = null);

        /// <summary>
        /// Returns an empty equipment slot with the given ID on this entity, if any.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="slotId"></param>
        /// <param name="equipSlotInstance"></param>
        /// <param name="equipSlotsComp"></param>
        /// <returns></returns>
        bool TryGetEmptyEquipSlot(EntityUid uid, PrototypeId<EquipSlotPrototype> slotId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            EquipSlotsComponent? equipSlotsComp = null);

        /// <summary>
        /// Returns the container for the given equip slot instance, creating it if necessary.
        /// </summary>
        bool TryGetContainerForEquipSlot(EntityUid uid, EquipSlotInstance equipSlotInstance,
            [NotNullWhen(true)] out ContainerSlot? containerSlot,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null);

        /// <summary>
        /// Returns the equipment slot instance for this container, if it exists.
        /// </summary>
        bool TryGetEquipSlotForContainer(EntityUid uid, ContainerId containerId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null);

        bool TryGetContainerSlotEnumerator(EntityUid uid,
            out EquipSlotsSystem.ContainerSlotEnumerator containerSlotEnumerator,
            EquipSlotsComponent? equipSlotsComp = null);

        bool TryGetEquipSlots(EntityUid uid,
            [NotNullWhen(true)] out IList<EquipSlotInstance>? slotDefinitions,
            EquipSlotsComponent? equipSlotsComp = null);

        IList<EquipSlotInstance> GetEquipSlots(EntityUid uid,
            EquipSlotsComponent? equipSlotsComp = null);
    }
}