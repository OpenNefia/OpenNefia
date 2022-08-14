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
    public class CharSerializer : ITypeSerializer<char, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return char.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing char value: {node.Value}");
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            return new DeserializedValue<char>(char.Parse(node.Value));
        }

        public DataNode Write(ISerializationManager serializationManager, char value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString(CultureInfo.InvariantCulture));
        }

        public char Copy(ISerializationManager serializationManager, char source, char target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, char left, char right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
