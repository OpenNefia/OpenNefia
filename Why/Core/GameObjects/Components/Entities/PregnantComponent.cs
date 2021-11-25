using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Serialization.Manager.Attributes;

namespace Why.Core.GameObjects
{
    [RegisterComponent]
    public class PregnantComponent : Component
    {
        public override string Name => "Pregnant";
    }
}