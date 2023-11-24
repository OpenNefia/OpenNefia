using System.Globalization;
using JetBrains.Annotations;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Serialization.Markdown.Value
{
    public sealed class ValueDataNode : DataNode<ValueDataNode>, IEquatable<ValueDataNode>
    {
        public ValueDataNode() : this(string.Empty) { }

        public ValueDataNode(string value, string? tag = null, ScalarStyle style = ScalarStyle.Any) : base(NodeMark.Invalid, NodeMark.Invalid)
        {
            Value = value;
            Tag = tag;
            Style = style;
        }

        public ValueDataNode(YamlScalarNode node) : base(node.Start, node.End)
        {
            Value = node.Value ?? string.Empty;
            Tag = node.Tag.IsEmpty ? null : node.Tag.Value;
            Style = node.Style;
        }

        public string Value { get; set; }

        public ScalarStyle Style { get; set; } = ScalarStyle.Any;

        public override bool IsEmpty => Value == string.Empty;

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

        public override ValueDataNode PushInheritance(ValueDataNode node)
        {
            return Copy();
        }

        public override bool Equals(object? obj)
        {
            return obj is ValueDataNode node && Equals(node);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        [Pure]
        public int AsInt()
        {
            return int.Parse(Value, CultureInfo.InvariantCulture);
        }

        [Pure]
        public float AsFloat()
        {
            return float.Parse(Value, CultureInfo.InvariantCulture);
        }

        [Pure]
        public bool AsBool()
        {
            return bool.Parse(Value);
        }

        public bool Equals(ValueDataNode? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }
    }
}