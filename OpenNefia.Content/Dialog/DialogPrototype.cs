using OpenNefia.Core;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Dialog
{
    [Prototype("Elona.Dialog")]
    public class DialogPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public string StartNode { get; } = string.Empty;

        [DataField("nodes", required: true)]
        private Dictionary<string, IDialogNode> _nodes = new();

        public IReadOnlyDictionary<string, IDialogNode> Nodes => _nodes;
    }

    public delegate void DialogNodeDelegate();

    [ImplicitDataDefinitionForInheritors]
    public interface IDialogNode
    {
        DialogNodeDelegate? BeforeEnter { get; }
        DialogNodeDelegate? AfterEnter { get; }
    }

    public sealed class TextNode : IDialogNode
    {
        [DataField("choices", required: true)]
        private List<TextNodeChoice> _choices { get; } = new();

        public IReadOnlyList<TextNodeChoice> Choices => _choices;

        [DataField]
        public DialogNodeDelegate? BeforeEnter { get; }

        [DataField]
        public DialogNodeDelegate? AfterEnter { get; }
    }

    [DataDefinition]
    public sealed class TextNodeChoice
    {
        [DataField]
        public string Target { get; set; } = string.Empty;

        [DataField]
        public LocaleKey Key { get; set; }
    }
}
