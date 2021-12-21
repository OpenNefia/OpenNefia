using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Base class for components that can be put into slots.
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Slots" are a collection of components that can be instantiated from a prototype 
    /// and bound on-demand by a parent component to an entity, and then unbound at a later time.
    /// </para>
    /// <para>
    /// Slots are supposed to solve three problems:
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
    /// 2. Allowing for one component type to have multiple instances 
    ///    or "stacks". Item enchantments that enhance skills can be stacked by
    ///    wearing more than one piece of equipment that offers the enchantment.
    ///    If these enchantments used the slots system to implement their logic,
    ///    there's no way to account for enchantments with the same child component
    ///    types but different power levels, because the entity system only allows
    ///    for a single component type instance per entity. A <see cref="SlottableComponent{T}"/>
    ///    in particular would need to have some kind of "merging" mechanism for 
    ///    calculating the final power of the enchantment/effect from its multiple "instances".
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
    ///    the effect can't be implemented purely in terms of standard <see cref="Component"/>s.
    /// </para>
    /// <para>
    /// The basic idea was adapted from the talk "There Be Dragons: Entity Component Systems 
    /// for Roguelikes" by Thomas Biskup. (https://www.youtube.com/watch?v=fGLJC5UY2o4)
    /// </para>
    /// </remarks>
    public abstract class SlottableComponent<T> : Component where T : struct
    {
        /// <summary>
        /// Merged power level of this component.
        /// </summary>
        public T MergedPower { get; set; }
    }
}
