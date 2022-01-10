using OpenNefia.Core.Areas;
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
    public class AreaFloorIdSerializer : ITypeSerializer<AreaFloorId, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            return new DeserializedValue<AreaFloorId>(new(node.Value));
        }

        public DataNode Write(ISerializationManager serializationManager, AreaFloorId value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode((string)value);
        }

        public AreaFloorId Copy(ISerializationManager serializationManager, AreaFloorId source, AreaFloorId target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, AreaFloorId left, AreaFloorId right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
