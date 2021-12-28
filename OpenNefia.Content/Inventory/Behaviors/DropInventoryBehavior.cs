using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class DropInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly VerbSystem _verbSystem = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 2));

        public override string WindowTitle => Loc.Get("Elona.Inventory.Behavior.Drop.WindowTitle");

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new EntityInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.Get("Elona.Inventory.Behavior.Drop.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            var verb = new Verb(PickableSystem.VerbIDDrop);
            return _verbSystem.GetLocalVerbs(context.User, item).Contains(verb);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            var verb = new Verb(PickableSystem.VerbIDDrop);
            var result = _verbSystem.ExecuteVerb(context.User, item, verb);
            
            if (result == TurnResult.Succeeded)
                return new InventoryResult.Finished(result);

            return new InventoryResult.Continuing();
        }
    }
}
