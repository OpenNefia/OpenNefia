using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Inventory
{
    public abstract record InventoryResult
    {
        public sealed record Finished(TurnResult TurnResult) : InventoryResult
        {
            public override string ToString() => $"{nameof(Finished)}({TurnResult})";
        }
        public sealed record Continuing(EntityUid? SelectedItem = null) : InventoryResult
        {
            public override string ToString() => $"{nameof(Continuing)}()";
        }
    }
}
