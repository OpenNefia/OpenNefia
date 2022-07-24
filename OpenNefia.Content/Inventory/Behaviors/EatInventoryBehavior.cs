using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
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
    public class EatInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 8));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Eat.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Eat);

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new GroundInvSource(context.User);
            yield return new EntityInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Eat.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return _verbSystem.CanUseVerbOn(context.User, item, EdibleSystem.VerbIDEat);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            context.ShowInventoryWindow = false;

            var result = TurnResult.NoResult;
            if (_verbSystem.TryGetVerb(context.User, item, EdibleSystem.VerbIDEat, out var verb))
                result = verb.Act();

            return new InventoryResult.Finished(result);
        }
    }
}
