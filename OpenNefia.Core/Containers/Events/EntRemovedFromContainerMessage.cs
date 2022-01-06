using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Raised when an entity is removed from a container.
    /// </summary>
    [PublicAPI]
    public sealed class EntRemovedFromContainerMessage : ContainerModifiedMessage
    {
        public EntRemovedFromContainerMessage(EntityUid entity, IContainer container) : base(entity, container) { }
    }
}
