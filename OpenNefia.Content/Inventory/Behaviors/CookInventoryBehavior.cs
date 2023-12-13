using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Potion;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class CookInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IFoodSystem _foods = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 16));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Cook.WindowTitle");

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new EntityInventorySource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Cook.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return EntityManager.TryGetComponent<FoodComponent>(item, out var foodComponent)
                && foodComponent.FoodType != null
                && !_foods.IsCooked(item);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            return new InventoryResult.Finished(TurnResult.Succeeded);
        }
    }
}
