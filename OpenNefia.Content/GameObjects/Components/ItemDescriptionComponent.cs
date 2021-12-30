using OpenNefia.Content.Inventory;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    [RegisterComponent]
    public class ItemDescriptionComponent : Component
    {
        public override string Name => "ItemDescription";

        public List<ItemDescriptionEntry> Entries { get; } = new();
    }
}