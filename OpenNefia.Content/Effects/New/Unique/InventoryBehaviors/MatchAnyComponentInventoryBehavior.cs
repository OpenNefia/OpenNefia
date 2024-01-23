using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Element;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;
using OpenNefia.Content.Inventory;

namespace OpenNefia.Content.Effects
{
    public sealed class MatchAnyComponentInventoryBehavior : BaseInventoryBehavior
    {
        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 23, subId: 5));

        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Zap);

        public MatchAnyComponentInventoryBehavior(IEnumerable<Type> acceptedComponentTypes)
        {
            AcceptedComponentTypes = acceptedComponentTypes.ToHashSet();
        }

        public HashSet<Type> AcceptedComponentTypes { get; } 

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new InventoryInvSource(context.User);
            yield return new EquipmentInvSource(context.User);
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return EntityManager.GetComponents(item).Any(c => AcceptedComponentTypes.Contains(c.GetType()));
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            return new InventoryResult.Finished(TurnResult.Succeeded);
        }
    }
}
