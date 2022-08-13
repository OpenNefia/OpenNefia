using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
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
    public sealed class MoneyComponent : Component
    {
        public override string Name => "Money";

        [DataField]
        public int Gold { get; set; }

        [DataField]
        public int Platinum { get; set; }

        [DataField]
        public IntRange? InitialGold { get; set; }

        [DataField]
        public bool AlwaysDropsGoldOnDeath { get; set; }
    }
}
