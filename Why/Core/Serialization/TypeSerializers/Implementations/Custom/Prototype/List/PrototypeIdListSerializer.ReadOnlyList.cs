using System.Collections.Generic;
using JetBrains.Annotations;
using Why.Core.IoC;
using Why.Core.Prototypes;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Sequence;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.List
{
    public partial class PrototypeIdListSerializer<T> : ITypeSerializer<IReadOnlyList<string>, SequenceDataNode>
        where T : class, IPrototype
    {
        DataNode ITypeWriter<IReadOnlyList<string>>.Write(
            ISerializationManager serializationManager,
            IReadOnlyList<string> value,
            bool alwaysWrite,
            ISerializationContext? context)
        {
            return WriteInternal(serializationManager, value, alwaysWrite, context);
        }

        [MustUseReturnValue]
        IReadOnlyList<string> ITypeCopier<IReadOnlyList<string>>.Copy(
            ISerializationManager serializationManager,
            IReadOnlyList<string> source,
            IReadOnlyList<string> target,
            bool skipHook,
            ISerializationContext? context)
        {
            return new List<string>(source);
        }

        DeserializationResult ITypeReader<IReadOnlyList<string>, SequenceDataNode>.Read(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context)
        {
            var list = new List<string>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var result = _prototypeSerializer.Read(
                    serializationManager,
                    (ValueDataNode) dataNode,
                    dependencies,
                    skipHook,
                    context);

                list.Add((string) result.RawValue!);
                mappings.Add(result);
            }

            return new DeserializedCollection<IReadOnlyList<string>, string>(list, mappings,
                elements => new List<string>(elements));
        }

        ValidationNode ITypeValidator<IReadOnlyList<string>, SequenceDataNode>.Validate(
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
