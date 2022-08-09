using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class DrinkInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 8));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Drink.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Drink);

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            if (context.User == context.Target)
                yield return new GroundInvSource(context.Target);
            yield return new EntityInvSource(context.Target);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Drink.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return _verbSystem.CanUseVerbOn(context.User, item, DrinkableSystem.VerbTypeDrink);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            context.ShowInventoryWindow = false;

            var result = TurnResult.NoResult;
            if (_verbSystem.TryGetVerb(context.User, item, DrinkableSystem.VerbTypeDrink, out var verb))
                result = verb.Act();

            if (result != TurnResult.NoResult)
                return new InventoryResult.Finished(result);

            return new InventoryResult.Continuing();
        }
    }
}
