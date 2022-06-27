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
    /// <summary>
    /// A system for managing groups of components temporarily added to entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Slots" are a collection of components that can be instantiated from a prototype 
    /// and bound on-demand by a parent component to an entity, and then unbound at a later time.
    /// </para>
    /// <para>
    /// Slots are meant to solve three problems:
    /// </para>
    /// <para>
    /// 1. Components that can transiently add other components. An
    ///    example are status effects. Each status effect needs to be
    ///    tracked with a remaining duration, but can also cause new
    ///    gameplay logic to take effect. The go-to solution for adding new logic
    ///    would be the event bus, which operates on components, but
    ///    (in this system) a status effect instance with a turn duration isn't 
    ///    a component. Thus, some way of temporarily adding components to an entity
    ///    at runtime is necessary. Allowing child components also means the children can 
    ///    be decoupled from status effects entirely and reused, for example 
    ///    as an intrinsic buff/debuff to a character type. It also means that nearly
    ///    any arbitrary component can be turned into a status effect/enchantment/etc. by
    ///    simply adding it to the slot prototype.
    /// </para>
    /// <para>
    /// 2. Allowing for one component type to have multiple instances 
    ///    or "stacks". Item enchantments that enhance skills can be stacked by
    ///    wearing more than one piece of equipment that offers the enchantment.
    ///    If these enchantments used the component system alone to implement their logic,
    ///    there would be no way to account for enchantments with the same child component
    ///    types but different power levels, because the entity system only allows
    ///    for a single component type instance per entity. A <see cref="SlottableComponent"/>
    ///    in particular is a generalization of the "merging" logic found in HSP Elona for 
    ///    calculating the final power of enchantments/status effects from their multiple 
    ///    "instances".
    /// </para>
    /// <para>
    /// 3. Enabling all of these components to be hooked up to the
    ///    event bus system, *which is the important part*, because the dominating
    ///    paradigm for adding new game logic is to add new components and systems.
    ///    Without slots you'd probably end up hacking on a dynamic dispatch mechanism
    ///    for every "instance list" type component (enchantments, status effects) times
    ///    every event type those components need to support, which balloons into 
    ///    an NxM situation. Then you'd have to write "HasEnchantment()", "GetEnchantment()", 
    ///    "ListEnchantments()" and other methods for every such "instance list" system as well,
    ///    whereas keeping everything in terms of components is simpler. In other words, if
    ///    an enchantment can cause some sort of effect, then there ought to be no reason that
    ///    the effect can't be implemented purely in terms of standard <see cref="Component"/>s
    ///    if it *didn't* need to support stacking/transience.
    /// </para>
    /// <para>
    /// The basic idea was adapted from the talk "There Be Dragons: Entity Component Systems 
    /// for Roguelikes" by Thomas Biskup. (https://www.youtube.com/watch?v=fGLJC5UY2o4)
    /// </para>
    /// </remarks>
    public interface ISlotSystem : IEntitySystem
    {
        /// <summary>
        /// Adds a new slot to this entity from a set of components.
        /// </summary>
        /// <remarks>
        /// If the entity already has a subset of the components being registered,
        /// those components will not be added.
        /// </remarks>
        /// <param name="uid"></param>
        /// <param name="comps"></param>
        /// <param name="overwrite">If true, overwrite existing components on the entity.</param>
        /// <returns>The slot ID of the new slot.</returns>
        SlotId AddSlot(EntityUid uid, ComponentRegistry comps, bool overwrite = false);

        /// <summary>
        /// Removes an existing slot on an entity.
        /// </summary>
        /// <remarks>
        /// If any component types tracked by the slot have no other
        /// slots managing it afterward, the component instances on
        /// the entity will be removed.
        /// </remarks>
        /// <param name="uid">Entity to remove the slot from.</param>
        /// <param name="slotId">Slot to remove.</param>
        /// <param name="slots">Slots component to use.</param>
        /// <exception cref="ArgumentException">If the slot does not exist on the entity.</exception>
        void RemoveSlot(EntityUid uid, SlotId slotId, SlotsComponent? slots = null);

        /// <summary>
        /// Returns the ID of a slot managing this component, if one exists.
        /// </summary>
        /// <typeparam name="T">Type of component to check.</typeparam>
        /// <param name="uid">Entity to check on.</param>
        /// <param name="slots">Slots component to use.</param>
        /// <returns>The slot ID, if found.</returns>
        SlotId? FindSlotWithComponent<T>(EntityUid uid, SlotsComponent? slots = null) where T : IComponent;
    }

    public class SlotSystem : EntitySystem, ISlotSystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;

        /// <inheritdoc/>
        public SlotId AddSlot(EntityUid uid, ComponentRegistry comps, bool overwrite = false)
        {
            var addingComps = new ComponentRegistry();
            var removingComps = new ComponentRegistry();
            var slotRegCompTypes = new HashSet<Type>();

            var slots = EntityManager.EnsureComponent<SlotsComponent>(uid);

            foreach (var (name, comp) in comps)
            {
                var compType = comp.GetType();

                if (!EntityManager.HasComponent(uid, compType))
                {
                    addingComps.Add(name, comp);
                }
                else if (overwrite)
                {
                    removingComps.Add(name, comp);
                    addingComps.Add(name, comp);
                }

                // If the entity doesn't have the component, track it in the slot.
                //
                // If the entity has the component and it's being managed by a
                // different slot, then track it in the slot registration for bookkeeping
                // purposes, but don't add it twice.
                //
                // If the entity has the component, but it's *not* part of a slot
                // (e.g. it's in the entity's original component list), then don't track
                // it in the slot system at all.
                bool notInOriginalEntityComps = !EntityManager.HasComponent(uid, compType) || slots.HasAnySlotsWithComp(compType) || overwrite;
                if (notInOriginalEntityComps)
                {
                    slotRegCompTypes.Add(compType);
                }
            }

            foreach (var compProto in removingComps.Values)
            {
                EntityManager.RemoveComponent(uid, compProto.GetType());
            }

            foreach (var compProto in addingComps.Values)
            {
                var compType = compProto.GetType();

                var newCompInstance = (Component)_componentFactory.GetComponent(compType);
                var compCopy = (Component)_serializationManager.Copy(compProto, newCompInstance)!;

                compCopy.Owner = uid;

                EntityManager.AddComponent(uid, compCopy);
                slotRegCompTypes.Add(compType);
            }

            var slotId = slots.MaxSlotId;
            slots.MaxSlotId = new SlotId(slots.MaxSlotId.Value + 1);

            var slotReg = new SlotRegistration(slotId, slotRegCompTypes);
            slots._registrations.Add(slotId, slotReg);

            return slotId;
        }

        /// <inheritdoc/>
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
                // If all slots that manage this component type have been removed,
                // remove the component from the entity.
                // 
                // (A component type should only show up here if a slot is managing it,
                // such that the entity's original components won't get removed.)
                if (!slots.HasAnySlotsWithComp(compType) && EntityManager.HasComponent(uid, compType))
                {
                    EntityManager.RemoveComponent(uid, compType);
                }
            }

            if (slots.Registrations.Count == 0)
                EntityManager.RemoveComponent<SlotsComponent>(uid);
        }

        /// <inheritdoc/>
        public SlotId? FindSlotWithComponent<T>(EntityUid uid, SlotsComponent? slots = null)
            where T : IComponent
        {
            if (!EntityManager.HasComponent<T>(uid))
                return null;

            if (!Resolve(uid, ref slots))
                return null;

            foreach (var (slotId, reg) in slots.Registrations)
            {
                if (reg.CompTypes.Contains(typeof(T)))
                    return slotId;
            }

            return null;
        }
    }
}
