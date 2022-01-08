using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Inventory
{
    /// <summary>
    /// Asset region IDs for <see cref="AssetPrototypeOf.InventoryIcons"/>.
    /// </summary>
    public enum InventoryIcon : int
    {
        Drink = 0,
        Zap = 1,
        Eat = 2,
        Read = 3,
        Open = 4,
        Use = 5,
        DipSource = 6,
        Examine = 7,
        Drop = 8,
        Tome = 9, // Unused.
        Equip = 10,
        PickUp = 17,
        Throw = 18,
    }
}
