using YamlDotNet.RepresentationModel;

namespace Why.Core.Serialization.Markdown.Value
{
    public class ValueDataNode : DataNode<ValueDataNode>
    {
        public ValueDataNode(string value) : base(NodeMark.Invalid, NodeMark.Invalid)
        {
            Value = value;
        }

        public ValueDataNode(YamlScalarNode node) : base(node.Start, node.End)
        {
            Value = node.Value ?? string.Empty;
            Tag = node.Tag;
        }

        public string Value { get; set; }

        public override ValueDataNode Copy()
        {
            return new(Value)
            {
                Tag = Tag,
                Start = Start,
                End = End
            };
        }

        public override ValueDataNode? Except(ValueDataNode node)
        {
            return node.Value == Value ? null : Copy();
        }

        public override bool Equals(object? obj)
        {
            return obj is ValueDataNode node && node.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
