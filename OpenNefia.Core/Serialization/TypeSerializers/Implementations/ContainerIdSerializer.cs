using OpenNefia.Core.Containers;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype
{
    [TypeSerializer]
    public class ContainerIdSerializer : ITypeSerializer<ContainerId, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            return new DeserializedValue<ContainerId>(new(node.Value));
        }

        public DataNode Write(ISerializationManager serializationManager, ContainerId value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode((string)value);
        }

        public ContainerId Copy(ISerializationManager serializationManager, ContainerId source, ContainerId target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }
    }
}
