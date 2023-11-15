using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Value;

namespace OpenNefia.Core.Serialization
{
    public sealed class AnyDataNode
    {
        public AnyDataNode() { }
        public AnyDataNode(DataNode node) { Node = node; }
        public DataNode Node { get; } = new ValueDataNode("");

        public AnyDataNode Copy()
        {
            return new AnyDataNode(Node);
        }
    }
}
