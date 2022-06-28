using Content.Shared.Inventory.Events;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.EquipSlots.Events;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.EquipSlots.EquipSlotPrototype>;

namespace OpenNefia.Content.Equipment
{
    public interface IEquipmentSystem : IEntitySystem
    {
        /// <summary>
        /// Returns true if this entity can be equipped to the given equipment slot type.
        /// </summary>
        /// <param name="item">Entity to check.</param>
        /// <param name="equipSlotId">Equipment slot equipping to.</param>
        bool CanEquipItemInSlot(EntityUid item, EquipSlotPrototypeId equipSlotId, 
            EquipmentComponent? itemEquipment = null);
    }

    public class EquipmentSystem : EntitySystem, IEquipmentSystem
    {
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlotsSystem = default!;

        public static readonly SubId HandlerHandleRefresh = new SubId(typeof(EquipmentSystem), nameof(HandleRefresh));

        public override void Initialize()
        {
            SubscribeLocalEvent<EquipmentComponent, GotEquippedEvent>(HandleGotEquipped, nameof(HandleGotEquipped));
            SubscribeLocalEvent<EquipmentComponent, GotUnequippedEvent>(HandleGotUnequipped, nameof(HandleGotUnequipped));

            SubscribeLocalEvent<EquipSlotsComponent, DidEquipEvent>(HandleDidEquip, nameof(HandleDidEquip));
            SubscribeLocalEvent<EquipSlotsComponent, DidUnequipEvent>(HandleDidUnequip, nameof(HandleDidUnequip));
            SubscribeLocalEvent<EquipSlotsComponent, EntityRefreshEvent>(HandleRefresh, nameof(HandleRefresh));
        }

        #region Event Handlers

        private void HandleGotEquipped(EntityUid uid, EquipmentComponent component, GotEquippedEvent args)
        {
            _refresh.Refresh(uid);
        }

        private void HandleGotUnequipped(EntityUid uid, EquipmentComponent component, GotUnequippedEvent args)
        {
            _refresh.Refresh(uid);
        }

        private void HandleDidEquip(EntityUid uid, EquipSlotsComponent component, DidEquipEvent args)
        {
            _refresh.Refresh(uid);
        }

        private void HandleDidUnequip(EntityUid uid, EquipSlotsComponent component, DidUnequipEvent args)
        {
            _refresh.Refresh(uid);
        }

        private void HandleRefresh(EntityUid equipper, EquipSlotsComponent component, ref EntityRefreshEvent args)
        {
            if (_equipSlotsSystem.TryGetContainerSlotEnumerator(equipper, out var enumerator))
            {
                while (enumerator.MoveNext(out var containerSlot))
                {
                    var equipment = containerSlot.ContainedEntity;
                    if (EntityManager.IsAlive(equipment))
                    {
                        var ev = new ApplyEquipmentToEquipperEvent(equipper);
                        RaiseLocalEvent(equipment.Value, ref ev);
                    }
                }
            }
        }

        #endregion

        #region Querying

        /// <inheritdoc/>
        public bool CanEquipItemInSlot(EntityUid item, EquipSlotPrototypeId equipSlotId,
            EquipmentComponent? itemEquipment = null)
        {
            if (!Resolve(item, ref itemEquipment, logMissing: false))
                return false;

            return itemEquipment.EquipSlots.Contains(equipSlotId);
        }

        #endregion
    }

    [ByRefEvent]
    public struct ApplyEquipmentToEquipperEvent
    {
        /// <summary>
        /// Entity that has this item equipped.
        /// </summary>
        public EntityUid Equipper;

        public ApplyEquipmentToEquipperEvent(EntityUid equipper)
        {
            Equipper = equipper;
        }
    }
}
