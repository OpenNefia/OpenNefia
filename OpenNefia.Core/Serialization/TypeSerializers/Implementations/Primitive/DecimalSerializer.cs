using System.Globalization;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Primitive
{
    [TypeSerializer]
    public class DecimalSerializer : ITypeSerializer<decimal, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return decimal.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing decimal value: {node.Value}");
        }

        public decimal Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null, decimal value = default)
        {
            return decimal.Parse(node.Value, CultureInfo.InvariantCulture);
        }

        public DataNode Write(ISerializationManager serializationManager, decimal value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString(CultureInfo.InvariantCulture));
        }

        public decimal Copy(ISerializationManager serializationManager, decimal source, decimal target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, decimal left, decimal right, bool skipHook, 
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
