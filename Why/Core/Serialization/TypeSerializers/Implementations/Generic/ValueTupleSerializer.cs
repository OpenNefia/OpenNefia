using System;
using System.Collections.Generic;
using System.Linq;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Mapping;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations.Generic
{
    [TypeSerializer]
    public class ValueTupleSerializer<T1, T2> : ITypeSerializer<ValueTuple<T1, T2>, MappingDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (node.Children.Count != 1)
                throw new InvalidMappingException("Less than or more than 1 mappings provided to ValueTupleSerializer");

            var entry = node.Children.First();
            var v1 = serializationManager.ReadValueOrThrow<T1>(entry.Key, context, skipHook);
            var v2 = serializationManager.ReadValueOrThrow<T2>(entry.Value, context, skipHook);

            return new DeserializedValue<ValueTuple<T1, T2>>(new ValueTuple<T1, T2>(v1, v2));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            if (node.Children.Count != 1) return new ErrorNode(node, "More or less than 1 Mapping for ValueTuple found.");

            var entry = node.Children.First();
            var dict = new Dictionary<ValidationNode, ValidationNode>
            {
                {
                    serializationManager.ValidateNode(typeof(T1), entry.Key, context),
                    serializationManager.ValidateNode(typeof(T2), entry.Value, context)
                }
            };

            return new ValidatedMappingNode(dict);
        }

        public DataNode Write(ISerializationManager serializationManager, (T1, T2) value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var mapping = new MappingDataNode();

            mapping.Add(
                serializationManager.WriteValue(typeof(T1), value.Item1, alwaysWrite, context),
                serializationManager.WriteValue(typeof(T2), value.Item2, alwaysWrite, context)
            );

            return mapping;
        }

        public (T1, T2) Copy(ISerializationManager serializationManager, (T1, T2) source, (T1, T2) target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return (serializationManager.Copy(source.Item1, target.Item1)!,
                serializationManager.Copy(source.Item2, source.Item2)!);
        }
    }
}
