using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
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
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 1));

        public override bool EnableShortcuts => true;

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Examine.WindowTitle");

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new GroundInvSource(context.User);
            yield return new EntityInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Examine.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            var verbPickUp = new Verb(PickableSystem.VerbIDPickUp);
            var verbDrop = new Verb(PickableSystem.VerbIDDrop);

            var verbs = _verbSystem.GetLocalVerbs(context.User, item);
            return verbs.Contains(verbPickUp) || verbs.Contains(verbDrop);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            _uiManager.Query<ItemDescriptionLayer, EntityUid>(item);

            return new InventoryResult.Continuing();
        }
    }
}
