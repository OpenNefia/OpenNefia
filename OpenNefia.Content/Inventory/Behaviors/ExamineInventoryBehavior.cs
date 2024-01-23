using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Content.Prototypes.Protos;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;

namespace OpenNefia.Content.Inventory
{
    public class ExamineInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 1));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Examine.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Examine);

        public override bool EnableShortcuts => true;

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            if (context.User == context.Target)
                yield return new GroundInvSource(context.Target);
            yield return new EquipmentInvSource(context.Target);
            yield return new InventoryInvSource(context.Target);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Examine.QueryText");
        }

        private readonly HashSet<string> _verbs = new()
        {
            PickableSystem.VerbTypePickUp,
            PickableSystem.VerbTypeDrop
        };

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return _verbSystem.CanUseAnyVerbOn(context.User, item, _verbs);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            var entities = context.AllInventoryEntries.Select(e => e.ItemEntityUid).ToList();
            var result = _uiManager.Query<ItemDescriptionLayer, ItemDescriptionLayer.Args, ItemDescriptionLayer.Result>(new(item));

            int? selectedIndex = result.HasValue ? result.Value.SelectedIndexOnExit : null;
            EntityUid? selectedItem = selectedIndex != null ? entities[selectedIndex.Value] : null;

            return new InventoryResult.Continuing(SelectedItem: selectedItem);
        }

        private void ToggleNoDrop(IInventoryLayer layer, EntityUid item)
        {
            if (!EntityManager.TryGetComponent(item, out PickableComponent pickable))
                return;

            Sounds.Play(Sound.Ok1);

            if (pickable.IsNoDrop)
            {
                pickable.IsNoDrop = false;
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Examine.NoDrop.Unset", ("entity", item)));
            }
            else
            {
                pickable.IsNoDrop = true;
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Examine.NoDrop.Set", ("entity", item)));
            }

            layer.RefreshList(InventoryRefreshListKind.Redisplay);
        }

        public override void OnKeyBindDown(IInventoryLayer layer, GUIBoundKeyEventArgs args)
        {
            if (args.Function == ContentKeyFunctions.UIMode2)
            {
                if (layer.SelectedEntry != null)
                    ToggleNoDrop(layer, layer.SelectedEntry.ItemEntityUid);

                args.Handle();
            }
        }
    }
}
