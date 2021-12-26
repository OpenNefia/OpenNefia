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
    public class ByteSerializer : ITypeSerializer<byte, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return byte.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing byte value: {node.Value}");
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            return new DeserializedValue<byte>(byte.Parse(node.Value, CultureInfo.InvariantCulture));
        }

        public DataNode Write(ISerializationManager serializationManager, byte value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString(CultureInfo.InvariantCulture));
        }

        public byte Copy(ISerializationManager serializationManager, byte source, byte target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, byte left, byte right, bool skipHook, 
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
