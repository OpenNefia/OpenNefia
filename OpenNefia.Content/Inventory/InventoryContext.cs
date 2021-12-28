using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Inventory
{
    /// <summary>
    /// The underlying behavior of an inventory screen. Separating it like
    /// this simplifies the creation of item shortcuts, since all that is
    /// needed is creating the context and running its methods without
    /// needing to open any windows.
    /// </summary>
    public sealed class InventoryContext
    {
        public EntityUid User { get; }
        public IInventoryBehavior Behavior { get; internal set; }

        public InventoryContext(EntityUid user, IInventoryBehavior behavior)
        {
            EntitySystem.InjectDependencies(behavior);

            User = user;
            Behavior = behavior;
        }

        public string GetQueryText()
        {
            return Behavior.GetQueryText(this);
        }

        public void OnQuery()
        {
            Behavior.OnQuery(this);
        }

        public bool IsAccepted(EntityUid ent)
        {
            return Behavior.IsAccepted(this, ent);
        }

        public IEnumerable<IInventorySource> GetSources()
        {
            return Behavior.GetSources(this)
                .Select(source => EntitySystem.InjectDependencies(source));
        }
    }
}
