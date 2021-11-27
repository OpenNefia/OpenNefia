using System.Collections.Generic;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.List
{
    public partial class PrototypeIdListSerializer<T> : ITypeSerializer<IReadOnlyList<PrototypeId<T>>, SequenceDataNode>
        where T : class, IPrototype
    {
        DataNode ITypeWriter<IReadOnlyList<PrototypeId<T>>>.Write(
            ISerializationManager serializationManager,
            IReadOnlyList<PrototypeId<T>> value,
            bool alwaysWrite,
            ISerializationContext? context)
        {
            return WriteInternal(serializationManager, value, alwaysWrite, context);
        }

        [MustUseReturnValue]
        IReadOnlyList<PrototypeId<T>> ITypeCopier<IReadOnlyList<PrototypeId<T>>>.Copy(
            ISerializationManager serializationManager,
            IReadOnlyList<PrototypeId<T>> source,
            IReadOnlyList<PrototypeId<T>> target,
            bool skipHook,
            ISerializationContext? context)
        {
            return new List<PrototypeId<T>>(source);
        }

        DeserializationResult ITypeReader<IReadOnlyList<PrototypeId<T>>, SequenceDataNode>.Read(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context)
        {
            var list = new List<PrototypeId<T>>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var result = _prototypeSerializer.Read(
                    serializationManager,
                    (ValueDataNode) dataNode,
                    dependencies,
                    skipHook,
                    context);

                list.Add(new((string) result.RawValue!));
                mappings.Add(result);
            }

            return new DeserializedCollection<IReadOnlyList<PrototypeId<T>>, PrototypeId<T>>(list, mappings,
                elements => new List<PrototypeId<T>>(elements));
        }

        ValidationNode ITypeValidator<IReadOnlyList<PrototypeId<T>>, SequenceDataNode>.Validate(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context)
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
    }
}
