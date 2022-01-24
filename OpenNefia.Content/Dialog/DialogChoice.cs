using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    [Flags]
    public enum DialogChoiceFlag
    {
        None = 0,
        Branch = 1 << 0,
        More = 1 << 1,
        OverrideNode = 1 << 2,
    }

    [DataDefinition]
    public class DialogChoice
    {
        [DataField]
        public string LocKey { get; set; }

        [DataField]
        public int Priority { get; set; }

        [DataField(required: true)]
        public IDialogNode? Node { get; set; }

        [DataField]
        public DialogFormatData? ExtraFormat { get; set; }

        public DialogChoiceFlag Flags { get; set; }

        public DialogChoice()
        {
            LocKey = string.Empty;
            Node = null;
            ExtraFormat = null;
        }
        public DialogChoice(int priority = 0, PrototypeId<DialogNodePrototype>? id = null, string? locKey = null, DialogFormatData? extraFormat = null, DialogChoiceFlag flags = default)
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
                if (locKey == null)
                {
                    LocKey = proto.LocKey;
                }
            }

            if (locKey != null)
            {
                LocKey = locKey;
            }
            LocKey ??= "";

            ExtraFormat = extraFormat;
            Flags = flags;
        }
    }
}
