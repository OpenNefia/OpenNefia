using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI;
using OpenNefia.Core;
using OpenNefia.Content.Pickable;

namespace OpenNefia.Content.Inventory
{
    /// <summary>
    /// Defines the filtering/selection logic for a single inventory screen.
    /// </summary>
    public interface IInventoryBehavior : IHspIds<InvElonaId>
    {
        /// <summary>
        /// Whether to allow entries in this behavior to be bindable to shortcuts.
        /// </summary>
        bool EnableShortcuts { get; }

        /// <summary>
        /// Whether the player should be queried for an item count after selecting an item.
        /// </summary>
        bool QueryAmount { get; }

        /// <summary>
        /// Text to print when querying amount.
        /// </summary>
        LocaleKey? QueryAmountPrompt { get; }

        /// <summary>
        /// Whether to show the user's total weight in the inventory screen.
        /// </summary>
        bool ShowTotalWeight { get; }

        /// <summary>
        /// Whether to show the total amount of gold in the inventory screen.
        /// </summary>
        bool ShowMoney { get; }

        /// <summary>
        /// Whether to show the target's equipment in the inventory screen.
        /// </summary>
        bool ShowTargetEquip { get; }

        /// <summary>
        /// Window title to display.
        /// </summary>
        string WindowTitle { get; }

        /// <summary>
        /// The default amount to use when querying for an item amount.
        /// </summary>
        int DefaultAmount { get; }

        /// <summary>
        /// Whether to include items with a <see cref="OwnState.Special"/> own state in the filtered list.
        /// </summary>
        bool AllowSpecialOwned { get; }

        /// <summary>
        /// Whether to show "(Ground)" next to the names of items on the ground with this behavior.
        /// </summary>
        bool ApplyNameModifiers { get; }

        /// <summary>
        /// If true, restore list position when reopening this inventory screen.
        /// </summary>
        /// <remarks>
        /// In vanilla: disabled by the shop buying and selling screens.
        /// </remarks>
        bool RestorePreviousListIndex { get; }

        /// <summary>
        /// If non-null, automatically exit the inventory menu if there are no more filtered items. Used by the get/drop actions.
        /// </summary>
        TurnResult? TurnResultAfterSelectionIfEmpty { get; }

        /// <summary>
        /// Allocates the icon that this behavior will display in the icon bar.
        /// </summary>
        UiElement? MakeIcon();

        IEnumerable<IInventorySource> GetSources(InventoryContext context);

        string GetQueryText(InventoryContext context);

        string GetItemName(InventoryContext context, EntityUid item);

        string GetItemDetails(InventoryContext context, EntityUid item);

        bool IsAccepted(InventoryContext context, EntityUid item);

        void OnQuery(InventoryContext inventoryContext);

        int? OnQueryAmount(InventoryContext context, EntityUid item);

        InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount);

        InventoryResult AfterFilter(InventoryContext context, IReadOnlyList<InventoryEntry> filteredItems);

        void OnKeyBindDown(IInventoryLayer layer, GUIBoundKeyEventArgs args);

        List<UiKeyHint> MakeKeyHints();
    }

    public struct InvElonaId
    {
        /// <summary>
        /// ID of the inventory screen.
        /// </summary>
        public int Id;

        /// <summary>
        /// Sub ID of the inventory screen. For example, the "get"
        /// action has sub IDs for taking items from the ground
        /// and taking items from containers.
        /// </summary>
        public int SubId;

        public InvElonaId(int id, int subId = 0)
        {
            Id = id;
            SubId = subId;
        }
    }
}
