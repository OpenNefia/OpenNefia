using System.Collections.Generic;
using System.Collections.Immutable;
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
        ITypeSerializer<ImmutableList<PrototypeId<T>>, SequenceDataNode>
        where T : class, IPrototype
    {
        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return ValidateInternal(serializationManager, node, dependencies, context);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            var builder = ImmutableList.CreateBuilder<PrototypeId<T>>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var result = _prototypeSerializer.Read(
                    serializationManager,
                    (ValueDataNode) dataNode,
                    dependencies,
                    skipHook,
                    context);

                builder.Add(new((string) result.RawValue!));
                mappings.Add(result);
            }

            return new DeserializedCollection<ImmutableList<PrototypeId<T>>, PrototypeId<T>>(builder.ToImmutable(), mappings,
                ImmutableList.CreateRange);
        }

        public DataNode Write(ISerializationManager serializationManager, ImmutableList<PrototypeId<T>> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return WriteInternal(serializationManager, value, alwaysWrite, context);
        }

        public ImmutableList<PrototypeId<T>> Copy(ISerializationManager serializationManager, ImmutableList<PrototypeId<T>> source, ImmutableList<PrototypeId<T>> target,
            bool skipHook, ISerializationContext? context = null)
        {
            return ImmutableList.CreateRange(source);
        }
    }
}
