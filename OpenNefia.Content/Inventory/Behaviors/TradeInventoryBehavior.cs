using OpenNefia.Content.Pickable;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Inventory
{
    /// <seealso cref="PresentInventoryBehavior"/>
    public class TradeInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IPickableSystem _pickable = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 20));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Trade.WindowTitle");
        public override bool ApplyNameModifiers => false;
        public override bool QueryAmount => true;

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new InventoryInvSource(context.Target);
            yield return new EquipmentInvSource(context.Target);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Trade.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return !_tags.HasTag(item, Protos.Tag.ItemNotrade);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            var childContext = new InventoryContext(context.User, context.Target, new PresentInventoryBehavior(item, amount));
            var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(childContext);

            if (result.HasValue && result.Value.Data is InventoryResult.Finished invResult 
                && invResult.TurnResult == TurnResult.Succeeded)
                return new InventoryResult.Finished(TurnResult.Succeeded);

            return new InventoryResult.Finished(TurnResult.Aborted);
        }
    }
}
