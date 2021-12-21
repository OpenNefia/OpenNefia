using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Core.GameObjects
{
    public class SlotSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;

        public override void Initialize()
        {
        }

        /// <summary>
        /// Adds a new slot to this entity from a set of components.
        /// </summary>
        /// <remarks>
        /// If the entity already has a subset of the components being registered,
        /// those components will not be added.
        /// </remarks>
        /// <param name="uid"></param>
        /// <param name="comps"></param>
        /// <returns></returns>
        public SlotId AddSlot(EntityUid uid, ComponentRegistry comps)
        {
            var addingComps = new ComponentRegistry();
            var slotRegCompTypes = new HashSet<Type>();

            var slots = EntityManager.EnsureComponent<SlotsComponent>(uid);

            foreach (var (name, comp) in comps)
            {
                var compType = comp.GetType();

                // because AddComponent() requires Component, not IComponent
                if (!typeof(Component).IsAssignableFrom(compType))
                {
                    throw new InvalidDataException($"Passed component was not derived from Component: {compType}");
                }

                if (!EntityManager.HasComponent(uid, compType))
                {
                    addingComps.Add(name, comp);
                }

                // If the entity doesn't have the component, track it in the slot.
                //
                // If the entity has the component but it's being managed by a
                // different slot, then track it in the registration for bookkeeping
                // purposes, but don't add it twice.
                bool notInOriginalEntityComps = !EntityManager.HasComponent(uid, compType) || slots.HasAnySlotsWithComp(compType);
                if (notInOriginalEntityComps)
                {
                    slotRegCompTypes.Add(compType);
                }
            }

            foreach (var compProto in addingComps.Values)
            {
                var compType = compProto.GetType();

                var newCompInstance = (Component)_componentFactory.GetComponent(compType);
                var compCopy = (Component)_serializationManager.Copy(compProto, newCompInstance)!;

                compCopy.Owner = _entityManager.GetEntity(uid);

                EntityManager.AddComponent(uid, compCopy);
                slotRegCompTypes.Add(compType);
            }

            var slotId = slots.MaxSlotId;
            slots.MaxSlotId = new SlotId(slots.MaxSlotId.Value + 1);

            var slotReg = new SlotRegistration(slotId, slotRegCompTypes);
            slots._registrations.Add(slotId, slotReg);

            return slotId;
        }

        public void RemoveSlot(EntityUid uid, SlotId slotId,
            SlotsComponent? slots = null)
        {
            if (!Resolve(uid, ref slots))
                return;

            if (!slots.Registrations.TryGetValue(slotId, out var slotReg))
                throw new ArgumentException($"Slot instance not found on entity {uid}: {slotId}");

            slots._registrations.Remove(slotId);

            foreach (var compType in slotReg.CompTypes)
            {
                if (!slots.HasAnySlotsWithComp(compType) && EntityManager.HasComponent(uid, compType))
                {
                    EntityManager.RemoveComponent(uid, compType);
                }
            }

            if (slots.Registrations.Count == 0)
                EntityManager.RemoveComponent<SlotsComponent>(uid);
        }
    }
}
