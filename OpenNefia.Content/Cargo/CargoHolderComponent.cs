using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Cargo
{
    /// <summary>
    /// Indicates an entity that can hold cargo items in an <see cref="InventoryComponent"/>
    /// </summary>
    [RegisterComponent]
    public class CargoHolderComponent : Component
    {
        public override string Name => "CargoHolder";

        [DataField]
        public int InitialMaxCargoWeight { get; set; } = 0;

        /// <summary>
        /// Maximum cargo weight this entity can hold. Null means "unlimited".
        /// </summary>
        [DataField]
        public int? MaxCargoWeight { get; set; } = 0;
    }
}
