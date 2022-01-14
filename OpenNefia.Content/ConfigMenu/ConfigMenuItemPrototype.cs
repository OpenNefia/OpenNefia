using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ConfigMenu
{
    [Prototype("Elona.ConfigMenuItem")]
    public sealed class ConfigMenuItemPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public IConfigMenuNode Node { get; } = default!;
    }
}
