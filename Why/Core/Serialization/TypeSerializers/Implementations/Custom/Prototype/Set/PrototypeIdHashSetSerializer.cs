using System.Collections.Generic;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set
{
    public class PrototypeIdHashSetSerializer<T> : ITypeSerializer<HashSet<PrototypeId<T>>, SequenceDataNode> where T : class, IPrototype
    {
        private readonly PrototypeIdSerializer<T> _prototypeSerializer = new();

        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node, IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var list = new List<ValidationNode>();

            foreach (var dataNode in node.Sequence)
            {
                if (dataNode is not ValueDataNode value)
                {
                    list.Add(new ErrorNode(dataNode, $"Cannot cast node {dataNode} to ValueDataNode."));
                    continue;
                }

                list.Add(_prototypeSerializer.Validate(serializationManager, value, dependencies, context));
            }

            return new ValidatedSequenceNode(list);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, SequenceDataNode node, IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            var set = new HashSet<PrototypeId<T>>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var result = _prototypeSerializer.Read(
                    serializationManager,
                    (ValueDataNode) dataNode,
                    dependencies,
                    skipHook,
                    context);

                set.Add(new((string) result.RawValue!));
                mappings.Add(result);
            }

            return new DeserializedCollection<HashSet<PrototypeId<T>>, PrototypeId<T>>(set, mappings,
                elements => new HashSet<PrototypeId<T>>(elements));
        }

        public DataNode Write(ISerializationManager serializationManager, HashSet<PrototypeId<T>> value, bool alwaysWrite = false, ISerializationContext? context = null)
        {
            var list = new List<DataNode>();

            foreach (var str in value)
            {
                list.Add(_prototypeSerializer.Write(serializationManager, str, alwaysWrite, context));
            }

            return new SequenceDataNode(list);
        }

        public HashSet<PrototypeId<T>> Copy(ISerializationManager serializationManager, HashSet<PrototypeId<T>> source, HashSet<PrototypeId<T>> target, bool skipHook, ISerializationContext? context = null)
        {
            return new(source);
        }
    }
}
