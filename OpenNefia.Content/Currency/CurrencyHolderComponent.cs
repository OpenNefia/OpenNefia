using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Currency
{
    [RegisterComponent]
    public sealed class CurrencyHolderComponent : Component
    {
        public override string Name => "CurrencyHolder";

        [DataField]
        public int Gold { get; set; }

        [DataField]
        public int Platinum { get; set; }
    }
}
