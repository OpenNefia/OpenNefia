using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class ThrowInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 26));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Throw.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Throw);

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            if (context.User == context.Target)
                yield return new GroundInvSource(context.Target);
            yield return new EntityInventorySource(context.Target);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Throw.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return _verbSystem.CanUseVerbOn(context.User, item, ActionThrowSystem.VerbTypeThrow);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            context.ShowInventoryWindow = false;

            var result = TurnResult.NoResult;
            if (_verbSystem.TryGetVerb(context.User, item, ActionThrowSystem.VerbTypeThrow, out var verb))
                result = verb.Act();

            if (result != TurnResult.NoResult)
                return new InventoryResult.Finished(result);

            return new InventoryResult.Continuing();
        }
    }
}
