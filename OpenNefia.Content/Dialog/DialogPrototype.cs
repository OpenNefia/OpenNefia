﻿using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Dialog
{
    [Prototype("Elona.Dialog")]
    public class DialogPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <summary>
        /// Node to start on when the dialog is initiated.
        /// </summary>
        [DataField(required: true)]
        public string StartNode { get; } = "__start__";

        [DataField("nodes", required: true)]
        private Dictionary<string, IDialogNode> _nodes = new();

        /// <summary>
        /// Set of all nodes in this dialog.
        /// </summary>
        public IReadOnlyDictionary<string, IDialogNode> Nodes => _nodes;
    }
}
