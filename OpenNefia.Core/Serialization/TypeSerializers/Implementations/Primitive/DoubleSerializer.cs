using System.Globalization;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Primitive
{
    [TypeSerializer]
    public class DoubleSerializer : ITypeSerializer<double, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return double.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing double value: {node.Value}");
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            return new DeserializedValue<double>(double.Parse(node.Value, CultureInfo.InvariantCulture));
        }

        public DataNode Write(ISerializationManager serializationManager, double value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString(CultureInfo.InvariantCulture));
        }

        public double Copy(ISerializationManager serializationManager, double source, double target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, double left, double right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
