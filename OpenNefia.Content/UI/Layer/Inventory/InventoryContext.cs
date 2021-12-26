using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer.Inventory
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

        public InventoryContext(EntityUid user)
        {
            User = user;
        }
    }
}
