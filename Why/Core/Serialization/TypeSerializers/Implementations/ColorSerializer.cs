using JetBrains.Annotations;
using Why.Core.IoC;
using Why.Core.Maths;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations
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
    }
}
