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

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.List
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

                list.Add((PrototypeId<T>)result.RawValue!);
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
