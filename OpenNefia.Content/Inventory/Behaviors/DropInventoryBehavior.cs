using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.UI.Element;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class DropInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 2));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Drop.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Drop);

        public override bool QueryAmount => true;
        public override LocaleKey? QueryAmountPrompt => "Elona.Inventory.Behavior.Drop.HowMany";

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new EntityInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Drop.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            var verb = new Verb(PickableSystem.VerbIDDrop);
            return _verbSystem.GetLocalVerbs(context.User, item).Contains(verb);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            if (CheckNoDropAndMessage(item))
                return new InventoryResult.Continuing();

            var verb = new Verb(PickableSystem.VerbIDDrop);
            var result = _verbSystem.ExecuteVerb(context.User, item, verb);
            
            if (result != TurnResult.NoResult)
                return new InventoryResult.Finished(result);

            return new InventoryResult.Continuing();
        }
    }
}
