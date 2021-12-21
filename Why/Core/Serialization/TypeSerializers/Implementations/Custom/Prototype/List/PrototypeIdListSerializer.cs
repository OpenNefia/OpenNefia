﻿using System.Collections.Generic;
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
    public partial class PrototypeIdListSerializer<T> : ITypeSerializer<List<string>, SequenceDataNode> where T : class, IPrototype
    {
        private readonly PrototypeIdSerializer<T> _prototypeSerializer = new();

        private ValidationNode ValidateInternal(
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

        private DataNode WriteInternal(
            ISerializationManager serializationManager,
            IEnumerable<string> value,
            bool alwaysWrite,
            ISerializationContext? context)
        {
            var list = new List<DataNode>();

            foreach (var str in value)
            {
                list.Add(_prototypeSerializer.Write(serializationManager, str, alwaysWrite, context));
            }

            return new SequenceDataNode(list);
        }

        ValidationNode ITypeValidator<List<string>, SequenceDataNode>.Validate(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context)
        {
            return ValidateInternal(serializationManager, node, dependencies, context);
        }

        DeserializationResult ITypeReader<List<string>, SequenceDataNode>.Read(
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

            return new DeserializedCollection<List<string>, string>(list, mappings,
                elements => new List<string>(elements));
        }

        DataNode ITypeWriter<List<string>>.Write(
            ISerializationManager serializationManager,
            List<string> value,
            bool alwaysWrite,
            ISerializationContext? context)
        {
            return WriteInternal(serializationManager, value, alwaysWrite, context);
        }

        List<string> ITypeCopier<List<string>>.Copy(
            ISerializationManager serializationManager,
            List<string> source,
            List<string> target,
            bool skipHook,
            ISerializationContext? context)
        {
            return new(source);
        }
    }
}
