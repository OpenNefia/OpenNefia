using System;

namespace Why.Core.GameObjects
{
    /// <remarks>
    ///     Base component for the ECS system.
    ///     All discoverable implementations of IComponent must override the <see cref="Name" />.
    ///     Instances are dynamically instantiated by a <c>ComponentFactory</c>, and will have their IoC Dependencies resolved.
    /// </remarks>
    public interface IComponent
    {
        /// <summary>
        ///     The current lifetime stage of this component. You can use this to check
        ///     if the component is initialized or being deleted.
        /// </summary>
        ComponentLifeStage LifeStage { get; }

        /// <summary>
        ///     Entity that this component is attached to.
        /// </summary>
        IEntity Owner { get; }

        /// <summary>
        ///     Entity Uid that this component is attached to.
        /// </summary>
        EntityUid OwnerUid => Owner.Uid;

        /// <summary>
        /// Name of this component.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Component has been properly initialized.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        ///     This is true when the component is active.
        /// </summary>
        bool Running { get; }

        /// <summary>
        ///     True if the component has been removed from its owner, AKA deleted.
        /// </summary>
        bool Deleted { get; }
    }
}
