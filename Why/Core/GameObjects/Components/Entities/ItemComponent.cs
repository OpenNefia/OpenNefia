using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Serialization.Manager.Attributes;

namespace Why.Core.GameObjects
{
    [RegisterComponent]
    public class ItemComponent : Component
    {
        public override string Name => "Item";

        [DataField("value")]
        public int Value { get; }
    }
}
