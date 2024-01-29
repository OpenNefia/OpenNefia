using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Inventory;

namespace OpenNefia.Content.Effects
{
    public sealed class MatchComponentsInventoryBehavior : BaseInventoryBehavior
    {
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(Icon);

        public MatchComponentsInventoryBehavior(IEnumerable<Type> acceptedComponentTypes,
            MatchComponentKind matchKind = MatchComponentKind.All,
            InventoryIcon icon = InventoryIcon.PickUp)
        {
            AcceptedComponentTypes = acceptedComponentTypes.ToHashSet();
            MatchKind = matchKind;
            Icon = icon;
        }

        public HashSet<Type> AcceptedComponentTypes { get; }
        public InventoryIcon Icon { get; }
        public MatchComponentKind MatchKind { get; } = MatchComponentKind.Any;

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new InventoryInvSource(context.User);
            yield return new EquipmentInvSource(context.User);
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            var components = EntityManager.GetComponents(item);
            var filter = (IComponent c) => AcceptedComponentTypes.Contains(c.GetType());
            return MatchKind == MatchComponentKind.Any
                ? components.Any(filter)
                : components.All(filter);
        }
    }

    public enum MatchComponentKind
    {
        All,
        Any
    }
}
