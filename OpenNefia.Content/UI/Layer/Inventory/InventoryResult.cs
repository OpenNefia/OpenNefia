using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.UI.Layer.Inventory
{
    public abstract record InventoryResult
    {
        public sealed record Finished(TurnResult InnerValue) : InventoryResult
        {
            public override string ToString() => $"{nameof(Finished)}({InnerValue})";
        }
        public sealed record Continuing() : InventoryResult
        {
            public override string ToString() => $"{nameof(Continuing)}()";
        }
    }
}
