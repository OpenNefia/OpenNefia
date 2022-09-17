using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype
{
    public class PrototypeIdArraySerializer<TPrototype> : ITypeSerializer<string[], SequenceDataNode>,
        ITypeSerializer<string[], ValueDataNode> where TPrototype : class, IPrototype
    {
        protected virtual PrototypeIdSerializer<TPrototype> PrototypeSerializer => new();

        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return new ValidatedSequenceNode(node.Select(x =>
                x is ValueDataNode valueDataNode
                    ? PrototypeSerializer.Validate(serializationManager, valueDataNode, dependencies, context)
                    : new ErrorNode(x, $"Cannot cast node {x} to ValueDataNode.")).ToList());
        }

        public string[] Read(ISerializationManager serializationManager, SequenceDataNode node, IDependencyCollection dependencies,
            bool skipHook, ISerializationContext? context = null, string[]? value = default)
        {
            return node.Select(x =>
                    PrototypeSerializer.Read(serializationManager, (ValueDataNode)x, dependencies, skipHook, context).ToString())
                .ToArray();
        }

        public DataNode Write(ISerializationManager serializationManager, string[] value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.WriteValue(value, alwaysWrite, context);
        }

        public string[] Copy(ISerializationManager serializationManager, string[] source, string[] target, bool skipHook,
            ISerializationContext? context = null)
        {
            serializationManager.Copy(source, ref target, context, skipHook);
            return target;
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null) =>
            PrototypeSerializer.Validate(serializationManager, node, dependencies, context);

        public string[] Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook, ISerializationContext? context = null, string[]? value = default) =>
            new[] { PrototypeSerializer.Read(serializationManager, node, dependencies, skipHook, context).ToString() };

        public bool Compare(ISerializationManager serializationManager, string[] left, string[] right, bool skipHook, ISerializationContext? context = null)
        {
            return serializationManager.Compare(left, right, context, skipHook);
        }
    }
}