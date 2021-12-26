using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class ColorSerializer : ITypeSerializer<Color, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var deserializedColor = Color.TryFromName(node.Value, out var color)
                ? color :
                Color.FromHex(node.Value);

            return new DeserializedValue<Color>(deserializedColor);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return Color.TryFromName(node.Value, out _) || Color.TryFromHex(node.Value) != null
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing Color.");
        }

        public DataNode Write(ISerializationManager serializationManager, Color value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToHex());
        }

        [MustUseReturnValue]
        public Color Copy(ISerializationManager serializationManager, Color source, Color target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.R, source.G, source.B, source.A);
        }

        public bool Compare(ISerializationManager serializationManager, Color left, Color right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
