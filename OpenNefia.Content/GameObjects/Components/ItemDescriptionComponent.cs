using OpenNefia.Content.Inventory;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    [RegisterComponent]
    public class ItemDescriptionComponent : Component
    {
        public override string Name => "ItemDescription";

        [DataField(required: true)]
        public List<ItemDescriptionEntry> Entries { get; } = new();
    }
}