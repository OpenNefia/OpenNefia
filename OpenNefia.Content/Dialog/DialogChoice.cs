using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    [DataDefinition]
    public class DialogChoice
    {
        [DataField]
        public string LocKey { get; set; }

        [DataField]
        public int Priority { get; set; }

        [DataField(required: true)]
        public IDialogNode Node { get; set; } = default!;

        [DataField]
        public List<DialogFormatData>? FormatData { get; set; }

        public DialogChoice()
        {
            LocKey = string.Empty;
            FormatData = null;
        }
        public DialogChoice(int priority = 0, PrototypeId<DialogNodePrototype>? id = null, string? locKey = null, IEnumerable<DialogFormatData>? formatData = null)
        {
            var proto = id?.ResolvePrototype();
            Priority = priority;
            if (proto != null)
            {
                Node = proto.Node;
                if (Node.LocKey == null)
                {
                    Node.LocKey = proto.LocKey;
                }
            }
            LocKey ??= proto?.LocKey ?? locKey ?? string.Empty;

            FormatData = formatData?.ToList();
        }
    }
}
