using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Inventory
{
    public interface IInventorySource
    {
        void OnDraw();
        IEnumerable<EntityUid> GetEntities();
        void ModifyEntityName(ref string name) { }
    }
}