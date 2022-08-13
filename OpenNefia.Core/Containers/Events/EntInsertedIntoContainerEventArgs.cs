using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Raised when an entity is inserted into a container.
    /// </summary>
    [PublicAPI]
    public sealed class EntInsertedIntoContainerEventArgs : ContainerModifiedEventArgs
    {
        public EntInsertedIntoContainerEventArgs(EntityUid entity, IContainer container) : base(entity, container) { }
    }
}
