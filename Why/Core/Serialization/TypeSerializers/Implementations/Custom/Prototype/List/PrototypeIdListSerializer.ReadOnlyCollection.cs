using System.Collections.Generic;
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
    public partial class PrototypeIdListSerializer<T> :
        ITypeSerializer<IReadOnlyCollection<PrototypeId<T>>, SequenceDataNode>
        where T : class, IPrototype
    {
        ValidationNode ITypeValidator<IReadOnlyCollection<PrototypeId<T>>, SequenceDataNode>.Validate(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context)
        {
            return ValidateInternal(serializationManager, node, dependencies, context);
        }

        DeserializationResult ITypeReader<IReadOnlyCollection<PrototypeId<T>>, SequenceDataNode>.Read(
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

            return new DeserializedCollection<List<PrototypeId<T>>, PrototypeId<T>>(list, mappings,
                elements => new List<PrototypeId<T>>(elements));
        }

        DataNode ITypeWriter<IReadOnlyCollection<PrototypeId<T>>>.Write(
            ISerializationManager serializationManager,
            IReadOnlyCollection<PrototypeId<T>> value,
            bool alwaysWrite,
            ISerializationContext? context)
        {
            return WriteInternal(serializationManager, value, alwaysWrite, context);
        }

        IReadOnlyCollection<PrototypeId<T>> ITypeCopier<IReadOnlyCollection<PrototypeId<T>>>.Copy(
            ISerializationManager serializationManager,
            IReadOnlyCollection<PrototypeId<T>> source,
            IReadOnlyCollection<PrototypeId<T>> target,
            bool skipHook,
            ISerializationContext? context)
        {
            return new List<PrototypeId<T>>(source);
        }
    }
}
