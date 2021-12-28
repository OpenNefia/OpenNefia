using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Inventory
{
    public abstract class BaseInventoryBehavior : IInventoryBehavior
    {
        [Dependency] protected readonly IEntityManager EntityManager = default!;

        public abstract HspIds<InvElonaId>? HspIds { get; }
        public abstract string WindowTitle { get; }

        public virtual bool EnableShortcuts => false;
        public virtual PrototypeId<AssetPrototype>? Icon => null;
        public virtual bool QueryAmount => false;
        public virtual bool ShowTotalWeight => true;
        public virtual bool ShowMoney => false;
        public virtual bool ShowTargetEquip => false;
        public virtual int DefaultAmount => 1;
        public virtual bool AllowSpecialOwned => false;

        /// <inheritdoc/>
        public abstract IEnumerable<IInventorySource> GetSources(InventoryContext context);

        /// <inheritdoc/>
        public virtual bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return true;
        }

        public virtual string GetQueryText(InventoryContext context)
        {
            return string.Empty;
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
    }
}
