using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Raised when an entity is removed from a container.
    /// </summary>
    [PublicAPI]
    public sealed class EntRemovedFromContainerEventArgs : ContainerModifiedEventArgs
    {
        public EntRemovedFromContainerEventArgs(EntityUid entity, IContainer container) : base(entity, container) { }
    }
}
