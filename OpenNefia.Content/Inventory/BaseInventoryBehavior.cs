using OpenNefia.Content.Cargo;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.UI;
using OpenNefia.Content.Weight;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Inventory
{
    public abstract class BaseInventoryBehavior : IInventoryBehavior
    {
        [Dependency] protected readonly IEntityManager EntityManager = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public abstract HspIds<InvElonaId>? HspIds { get; }
        public abstract string WindowTitle { get; }

        public virtual bool EnableShortcuts => false;
        public virtual bool QueryAmount => false;
        public virtual bool ShowTotalWeight => true;
        public virtual bool ShowMoney => false;
        public virtual bool ShowTargetEquip => false;
        public virtual int DefaultAmount => 1;
        public virtual bool AllowSpecialOwned => false;
        public virtual LocaleKey? QueryAmountPrompt => null;
        public virtual bool ApplyNameModifiers => true;
        public virtual bool ExitAfterSelectionIfEmpty => false;

        /// <inheritdoc/>
        public abstract IEnumerable<IInventorySource> GetSources(InventoryContext context);

        public virtual UiElement? MakeIcon()
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return true;
        }

        public virtual string GetQueryText(InventoryContext context)
        {
            return string.Empty;
        }

        public virtual string GetItemName(InventoryContext context, EntityUid item)
        {
            return Loc.GetString("Elona.Common.NameWithDirectArticle", ("entity", item));
        }

        public virtual string GetItemDetails(InventoryContext context, EntityUid item)
        {
            int? weight = null;

            if (EntityManager.TryGetComponent(item, out WeightComponent? weightComp))
            {
                weight = weightComp.Weight.Buffed;
            }
            if (EntityManager.TryGetComponent(item, out CargoComponent? cargoComp))
            {
                weight = cargoComp.CargoWeight;
            }

            if (weight != null)
            {
                return UiUtils.DisplayWeight(weight.Value);
            }

            return "-";
        }

        public virtual void OnQuery(InventoryContext context)
        {
        }

        public virtual InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            return new InventoryResult.Finished(TurnResult.Succeeded);
        }

        public virtual InventoryResult AfterFilter(InventoryContext context, IReadOnlyList<EntityUid> filteredItems)
        {
            return new InventoryResult.Continuing();
        }

        public virtual void OnKeyBindDown(IInventoryLayer layer, GUIBoundKeyEventArgs args)
        {
        }

        public virtual List<UiKeyHint> MakeKeyHints()
        {
            return new List<UiKeyHint>();
        }
    }
}
