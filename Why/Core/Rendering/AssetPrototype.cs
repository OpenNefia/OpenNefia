using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    [Prototype("Asset", -1)]
    public class AssetPrototype : IPrototype
    {
        [DataField("id")]
        public string ID { get; private set; } = default!;
    }
}
