using Content.Shared.Inventory.Events;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.EquipSlots.Events;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Weight;
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

        int GetTotalEquipmentWeight(EntityUid equipTarget, EquipSlotsComponent? equipSlotsComp = null);

        PrototypeId<SkillPrototype> GetArmorClass(int weight);
        PrototypeId<SkillPrototype> GetArmorClass(EntityUid ent, EquipSlotsComponent? equipSlotsComp = null);
    }

    public class EquipmentSystem : EntitySystem, IEquipmentSystem
    {
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlotsSystem = default!;

        public override void Initialize()
        {
            SubscribeComponent<EquipmentComponent, GotEquippedEvent>(HandleGotEquipped, priority: EventPriorities.Highest);
            SubscribeComponent<EquipmentComponent, GotUnequippedEvent>(HandleGotUnequipped, priority: EventPriorities.Highest);

            SubscribeComponent<EquipSlotsComponent, DidEquipEvent>(HandleDidEquip, priority: EventPriorities.Highest);
            SubscribeComponent<EquipSlotsComponent, DidUnequipEvent>(HandleDidUnequip, priority: EventPriorities.Highest);
            SubscribeComponent<EquipSlotsComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.VeryLow);
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
                        RaiseEvent(equipment.Value, ref ev);
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

        public int GetTotalEquipmentWeight(EntityUid equipTarget, EquipSlotsComponent? equipSlotsComp = null)
        {
            if (!_equipSlotsSystem.TryGetEquipSlots(equipTarget, out var equipSlots, equipSlotsComp))
                return 0;

            var totalWeight = 0;

            foreach (var equipSlot in equipSlots)
            {
                if (!_equipSlotsSystem.TryGetContainerForEquipSlot(equipTarget, equipSlot, out var containerSlot))
                    continue;

                if (!EntityManager.IsAlive(containerSlot.ContainedEntity))
                    continue;

                var equipment = containerSlot.ContainedEntity.Value;

                if (!EntityManager.TryGetComponent(equipment, out WeightComponent weight))
                    continue;

                totalWeight += weight.Weight;
            }

            return totalWeight;
        }

        public PrototypeId<SkillPrototype> GetArmorClass(EntityUid ent, EquipSlotsComponent? equipSlotsComp = null)
        {
            return GetArmorClass(GetTotalEquipmentWeight(ent, equipSlotsComp));
        }

        public PrototypeId<SkillPrototype> GetArmorClass(int weight)
        {
            if (weight >= EquipmentConstants.ArmorClassHeavyWeight)
            {
                return Protos.Skill.HeavyArmor;
            }
            else if (weight >= EquipmentConstants.ArmorClassMediumWeight)
            {
                return Protos.Skill.MediumArmor;
            }
            else
            {
                return Protos.Skill.LightArmor;
            }
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
