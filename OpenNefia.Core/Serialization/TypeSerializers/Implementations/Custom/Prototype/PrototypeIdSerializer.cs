using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype
{
    [TypeSerializer]
    public class PrototypeIdSerializer<TPrototype> : ITypeSerializer<PrototypeId<TPrototype>, ValueDataNode> where TPrototype : class, IPrototype
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return dependencies.Resolve<IPrototypeManager>().HasIndex(new PrototypeId<TPrototype>(node.Value))
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"PrototypeId<{typeof(TPrototype)}> for {node.Value} not found");
        }

        public PrototypeId<TPrototype> Read(ISerializationManager serializationManager, ValueDataNode node, IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null, PrototypeId<TPrototype> value = default)
        {
            return new PrototypeId<TPrototype>(new(node.Value));
        }

        public DataNode Write(ISerializationManager serializationManager, PrototypeId<TPrototype> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode((string)value);
        }

        public PrototypeId<TPrototype> Copy(ISerializationManager serializationManager, PrototypeId<TPrototype> source, PrototypeId<TPrototype> target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, PrototypeId<TPrototype> left, PrototypeId<TPrototype> right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
