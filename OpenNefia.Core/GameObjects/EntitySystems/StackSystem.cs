using OpenNefia.Analyzers;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Entity stacking.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This differs from Robust's StackSystem in several notable ways:
    /// </para>
    /// <para>
    /// 1. Robust's stacking system spawns a new entity when the stack is split.
    ///    In contrast, Elona's item_separate copies every property of the original
    ///    item to the split item.
    /// </para>
    /// <para>
    /// 2. Robust uses a "stackType" property to check if two entities can be
    ///    stacked together. If the stackType matches, then all is good. On the other
    ///    hand, Elona's item_stack does the equivalent of a deepcompare on every item
    ///    property to check for stackability, with special exceptions for some properties 
    ///    like quantity and quality. Naturally, this makes things way harder, especially
    ///    considering that some components were never meant to be stacked as part of an item.
    /// </para>
    /// <para>
    /// 3. Robust has an enforced maximum entity count per stack, while Elona lacks one.
    /// </para>
    /// </remarks>
    public class StackSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<StackComponent, MapInitEvent>(HandleEntityInitialized, nameof(HandleEntityInitialized));
        }

        /// <summary>
        /// Ensure that the liveness based on stackable amount is properly initialized.
        /// It has a dependency on <see cref="MetaDataComponent"/>.
        /// </summary>
        private void HandleEntityInitialized(EntityUid uid, StackComponent stackable, ref MapInitEvent args)
        {
            MetaDataComponent? metaData = null;

            if (!Resolve(uid, ref metaData))
                return;

            if (stackable.Count <= 0)
            {
                stackable.Count = 0;
                metaData.Liveness = EntityGameLiveness.DeadAndBuried;
            }
        }

        /// <summary>
        /// Returns true if the provided entities can be stacked together.
        /// </summary>
        public bool CanStack(EntityUid ent1, EntityUid ent2,
            StackComponent? stackEnt1 = null,
            StackComponent? stackEnt2 = null)
        {
            if (!EntityManager.IsAlive(ent1) || !EntityManager.IsAlive(ent2))
                return false;

            // Both entities must at least have StackComponents.
            if (!Resolve(ent1, ref stackEnt1) || !Resolve(ent2, ref stackEnt2))
                return false;

            foreach (var comp1 in EntityManager.GetComponents(ent1))
            {
                var compType = comp1.GetType();

                if (!EntityManager.TryGetComponent(ent2, compType, out var comp2))
                {
                    return false;
                }

                if (!_serializationManager.Compare(comp1, comp2))
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to stack the provided entities.
        /// </summary>
        /// <param name="target">Entity to stack into. This entity's stack count could be incremented.</param>
        /// <param name="with">Entity to stack with. This entity will be deleted if the stack succeeds.</param>
        /// <returns>True if the stack succeeded.</returns>
        public bool TryStack(EntityUid target, EntityUid with,
            StackComponent? stackTarget = null,
            StackComponent? stackWith = null)
        {
            if (!Resolve(target, ref stackTarget) || !Resolve(with, ref stackWith))
                return false;

            if (!CanStack(target, with, stackTarget, stackWith))
                return false;

            DebugTools.Assert(stackWith.Count > 0);

            var ev = new EntityStackedEvent(with);
            RaiseLocalEvent(target, ref ev);

            stackTarget.Count += stackWith.Count;
            stackWith.Count = 0;
            EntityManager.DeleteEntity(with);

            return true;
        }

        /// <summary>
        /// Tries to stack this entity with all entities on the same tile.
        /// </summary>
        /// <param name="target">Entity to stack.</param>
        /// <param name="stackTarget">Stackable component of the entity.</param>
        /// <returns>True if any stacking occurred.</returns>
        public bool TryStackOnSameTile(EntityUid target,
            SpatialComponent? spatialTarget = null,
            StackComponent? stackTarget = null)
        {
            if (!Resolve(target, ref spatialTarget, ref stackTarget))
                return false;

            var coords = spatialTarget.Coordinates;
            var stackedSomething = false;

            foreach (var ent in _lookup.GetLiveEntitiesAtCoords(coords))
            {
                stackedSomething &= TryStack(target, ent.Uid, stackTarget);
            }

            return stackedSomething;
        }
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct EntityStackedEvent
    {
        public EntityUid StackedWith { get; }

        public EntityStackedEvent(EntityUid stackingWith)
        {
            StackedWith = stackingWith;
        }
    }
}
