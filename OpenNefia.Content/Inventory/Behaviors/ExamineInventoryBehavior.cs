using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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
    public class ExamineInventoryBehavior : BaseInventoryBehavior
    {
        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 1));

        public override bool EnableShortcuts => true;

        public override string WindowTitle => nameof(GetInventoryBehavior);

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new GroundInvSource(context.User);
            yield return new EntityInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return "Examine what?";
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return true;
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            return new InventoryResult.Continuing();
        }
    }
}
