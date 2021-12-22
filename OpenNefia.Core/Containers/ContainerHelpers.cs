using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Helpers for the container system.
    /// </summary>
    /// <remarks>
    /// These are merely a set of proxy methods that <see cref="BaseContainer"/> 
    /// relies on internally.
    /// </remarks>
    public class ContainerHelpers
    {
        /// <summary>
        /// Tries to find the container manager that this entity is inside (if any).
        /// </summary>
        /// <param name="entity">Entity that might be inside a container.</param>
        /// <param name="manager">The container manager that this entity is inside of.</param>
        /// <returns>If a container manager was found.</returns>
        public static bool TryGetContainerMan(EntityUid entity, [NotNullWhen(true)] out ContainerManagerComponent? manager)
        {
            return EntitySystem.Get<ContainerSystem>().TryGetContainerMan(entity, out manager);
        }

        public static void AttachParentToContainerOrGrid(SpatialComponent transform)
        {
            EntitySystem.Get<ContainerSystem>().AttachParentToContainerOrGrid(transform);
        }

        public static bool RemoveEntity(EntityUid uid, EntityUid containedUid, bool force = false, ContainerManagerComponent? containerManager = null)
        {
            return EntitySystem.Get<ContainerSystem>().RemoveEntity(uid, containedUid, force, containerManager);
        }
    }
}
