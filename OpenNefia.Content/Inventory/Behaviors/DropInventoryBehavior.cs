using OpenNefia.Content.Pickable;
﻿using OpenNefia.Content.GameObjects;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class DropInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IPickableSystem _pickable = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 2));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Drop.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Drop);
        public override TurnResult? TurnResultAfterSelectionIfEmpty => TurnResult.Aborted;
        public override bool BlockInWorldMap => true;

        public override bool QueryAmount => true;
        public override LocaleKey? QueryAmountPrompt => "Elona.Inventory.Behavior.Drop.HowMany";

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new EntityInventorySource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Drop.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return _verbSystem.CanUseVerbOn(context.User, item, PickableSystem.VerbTypeDrop);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            if (!_pickable.CheckNoDropAndMessage(item))
                return new InventoryResult.Continuing();

            var result = TurnResult.NoResult;
            if (_verbSystem.TryGetVerb(context.User, item, PickableSystem.VerbTypeDrop, out var verb))
                result = verb.Act();
            
            if (result != TurnResult.NoResult)
                return new InventoryResult.Finished(result);

            return new InventoryResult.Continuing();
        }
    }
}
