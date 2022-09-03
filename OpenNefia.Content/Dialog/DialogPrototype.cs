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

        [DataField(required: true)]
        public string StartNode { get; } = "__start__";

        [DataField("nodes", required: true)]
        private Dictionary<string, IDialogNode> _nodes = new();

        public IReadOnlyDictionary<string, IDialogNode> Nodes => _nodes;

        [DataField("scriptImports")]
        private List<string> _scriptImports { get; set; } = new();
        public IReadOnlyList<string> ScriptImports => _scriptImports;

        [DataField("scriptDependencies")]
        private Dictionary<string, Type> _scriptDependencies { get; set; } = new();
        public IReadOnlyDictionary<string, Type> ScriptDependencies => _scriptDependencies;

        [DataField]
        public string? ScriptHeader { get; }

        internal object? ScriptObject { get; set; }
    }
}
