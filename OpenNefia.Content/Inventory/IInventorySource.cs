using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Inventory
{
    public interface IInventorySource
    {
        void OnDraw(float uiScale, float x, float y);
        IEnumerable<EntityUid> GetEntities();
        void ModifyEntityName(ref string name) {}
    }
}