using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Fired when a container should update the visiblity of the entities
    /// it contains within the map.
    /// </summary>
    public readonly struct UpdateContainerOcclusionEvent
    {
        public EntityUid Entity { get; }

        public UpdateContainerOcclusionEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }
}