using System.Collections.Generic;
using System.Linq;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype
{
    [TypeSerializer]
    public class PrototypeFlagsTypeSerializer<T>
        : ITypeSerializer<PrototypeFlags<T>, SequenceDataNode>, ITypeSerializer<PrototypeFlags<T>, ValueDataNode>
        where T : class, IPrototype
    {
        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var list = new List<ValidationNode>();

            foreach (var dataNode in node.Sequence)
            {
                if (dataNode is not ValueDataNode value)
                {
                    list.Add(new ErrorNode(dataNode, $"Cannot cast node {dataNode} to ValueDataNode."));
                    continue;
                }

                list.Add(serializationManager.ValidateNodeWith<PrototypeId<T>, PrototypeIdSerializer<T>, ValueDataNode>(value, context));
            }

            return new ValidatedSequenceNode(list);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            var flags = new List<PrototypeId<T>>(node.Sequence.Count);

            foreach (var dataNode in node.Sequence)
            {
                if (dataNode is not ValueDataNode value)
                    continue;

                flags.Add(new(value.Value));
            }

            return new DeserializedValue<PrototypeFlags<T>>(new PrototypeFlags<T>(flags));
        }

        public DataNode Write(ISerializationManager serializationManager, PrototypeFlags<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new SequenceDataNode(value.Select(x => (string)x).ToArray());
        }

        public PrototypeFlags<T> Copy(ISerializationManager serializationManager, PrototypeFlags<T> source, PrototypeFlags<T> target,
            bool skipHook, ISerializationContext? context = null)
        {
            return new PrototypeFlags<T>(source);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return serializationManager.ValidateNodeWith<PrototypeId<T>, PrototypeIdSerializer<T>, ValueDataNode>(node, context);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            return new DeserializedValue<PrototypeFlags<T>>(new PrototypeFlags<T>(new PrototypeId<T>(node.Value)));
        }
    }
}
