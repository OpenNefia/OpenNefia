using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    [Prototype("Elona.DialogNode")]
    public class DialogNodePrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField("locKey")]
        private string _LocKey { get; } = default!;
        public string LocKey => _LocKey ?? ID.Replace("Elona.", "Elona.Dialog.");

        [DataField]
        public IDialogNode Node { get; } = default!;
    }
}
