using OpenNefia.Content.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Containers;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.UI;
using OpenNefia.Content.Items;
using OpenNefia.Content.Weight;

namespace OpenNefia.Content.Effects
{
    public sealed class TakeFromPocketInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IPickableSystem _pickable = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IWeightSystem _weights = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 22, subId: 5));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Take.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.PickUp);
        public override bool ApplyNameModifiers => false;
        public override TurnResult? TurnResultAfterSelectionIfEmpty => TurnResult.Succeeded;

        public IContainer Container { get; }

        public TakeFromPocketInventoryBehavior(IContainer container) : base()
        {
            Container = container;
        }

        public override string GetTotalWeightDetails(InventoryContext context)
        {
            var comp = EntityManager.GetComponentOrNull<ItemContainerComponent>(Container.Owner);

            var currentItems = context.AllInventoryEntries.Count;

            var maxItems = comp?.MaxItemCount?.ToString() ?? "∞";

            var currentWeight = _weights.GetTotalWeight(Container.Owner, excludeSelf: true, stackCount: 1);
            var currentWeightStr = UiUtils.DisplayWeight(currentWeight);

            var maxWeight = comp?.MaxTotalWeight;
            var maxWeightStr = maxWeight != null ? UiUtils.DisplayWeight(maxWeight.Value) : "∞";

            var maxItemWeight = comp?.MaxItemWeight;
            var maxItemWeightStr = maxItemWeight != null ? UiUtils.DisplayWeight(maxItemWeight.Value) : "∞";

            var weightText = Loc.GetString("Elona.ItemContainer.FourDimensionalPocket.TotalWeight",
                ("currentItems", currentItems),
                ("maxItems", maxItems),
                ("currentWeight", currentWeightStr),
                ("maxWeight", maxWeightStr),
                ("maxItemWeight", maxItemWeightStr));

            return weightText;
        }

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new ContainerInvSource(Container);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Take.QueryText");
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            if (!_damages.DoStaminaCheck(context.User, 10))
            {
                _mes.Display(Loc.GetString("Elona.Common.TooExhausted", ("entity", context.User)));
                return new InventoryResult.Finished(TurnResult.Succeeded);
            }

            if (!_pickable.CheckPickableOwnStateAndMessage(item))
                return new InventoryResult.Finished(TurnResult.Succeeded);

            var result = TurnResult.NoResult;
            if (_verbSystem.TryGetVerb(context.User, item, PickableSystem.VerbTypePickUp, out var verb))
                result = verb.Act();

            if (result == TurnResult.Failed || result == TurnResult.Aborted)
                return new InventoryResult.Finished(TurnResult.Succeeded);

            return new InventoryResult.Continuing();
        }
    }
}