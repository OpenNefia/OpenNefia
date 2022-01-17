using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Currency
{
    /// <summary>
    /// Indicates something that can hold gold and platinum coins, like
    /// characters.
    /// </summary>
    [RegisterComponent]
    public sealed class WalletComponent : Component
    {
        public override string Name => "Wallet";

        [DataField]
        public int Gold { get; set; }

        [DataField]
        public int Platinum { get; set; }
    }
}
