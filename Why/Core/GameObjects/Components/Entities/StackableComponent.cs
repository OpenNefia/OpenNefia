using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Serialization.Manager.Attributes;

namespace Why.Core.GameObjects
{
    [RegisterComponent]
    public class StackableComponent : Component
    {
        public override string Name => "Stackable";

        [DataField("amount")]
        public int Amount { get; set; } = 1;
    }
}
