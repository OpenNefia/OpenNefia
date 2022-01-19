using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    [Prototype("Elona.DialogItem")]
    public sealed class DialogItemPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;
    }
}
