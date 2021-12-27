using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.UI.Layer.Inventory
{
    public class GroundInvSource : IInventorySource
    {
        public EntityUid Entity { get; }

        public GroundInvSource(EntityUid entity)
        {
            Entity = entity;
        }

        public void OnDraw()
        {
        }
    }
}