using System.Collections.Generic;
using Why.Core.IoC;
using Why.Core.Prototypes;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Mapping;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Implementations.Generic;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary
{
    // [TypeSerializer]
    public class PrototypeIdDictionarySerializer<TValue, TPrototype> :
        ITypeSerializer<Dictionary<string, TValue>, MappingDataNode>,
        ITypeSerializer<SortedDictionary<string, TValue>, MappingDataNode>,
        ITypeSerializer<IReadOnlyDictionary<string, TValue>, MappingDataNode>
        where TPrototype : class, IPrototype
    {
        private readonly DictionarySerializer<string, TValue> _dictionarySerializer = new();
        private readonly PrototypeIdSerializer<TPrototype> _prototypeSerializer = new();

        private ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            var mapping = new Dictionary<ValidationNode, ValidationNode>();

            foreach (var (key, val) in node.Children)
            {
                if (key is not ValueDataNode value)
                {
                    mapping.Add(new ErrorNode(key, $"Cannot cast node {key} to ValueDataNode."), serializationManager.ValidateNode(typeof(TValue), val, context));
                    continue;
                }

                mapping.Add(_prototypeSerializer.Validate(serializationManager, value, dependencies, context), serializationManager.ValidateNode(typeof(TValue), val, context));
            }

            return new ValidatedMappingNode(mapping);
        }

        ValidationNode ITypeValidator<Dictionary<string, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, dependencies, context);
        }

        ValidationNode ITypeValidator<SortedDictionary<string, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, dependencies, context);
        }

        ValidationNode ITypeValidator<IReadOnlyDictionary<string, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, dependencies, context);
        }

        DeserializationResult ITypeReader<Dictionary<string, TValue>, MappingDataNode>.Read(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context)
        {
            return _dictionarySerializer.Read(serializationManager, node, dependencies, skipHook, context);
        }

        DeserializationResult ITypeReader<SortedDictionary<string, TValue>, MappingDataNode>.Read(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context)
        {
            return _dictionarySerializer.Read(serializationManager, node, dependencies, skipHook, context);
        }

        DeserializationResult ITypeReader<IReadOnlyDictionary<string, TValue>, MappingDataNode>.Read(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context)
        {
            return _dictionarySerializer.Read(serializationManager, node, dependencies, skipHook, context);
        }

        public DataNode Write(ISerializationManager serializationManager, Dictionary<string, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Write(serializationManager, value, alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, SortedDictionary<string, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Write(serializationManager, value, alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, IReadOnlyDictionary<string, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Write(serializationManager, value, alwaysWrite, context);
        }

        public Dictionary<string, TValue> Copy(ISerializationManager serializationManager,
            Dictionary<string, TValue> source, Dictionary<string, TValue> target, bool skipHook,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Copy(serializationManager, source, target, skipHook, context);
        }

        public SortedDictionary<string, TValue> Copy(ISerializationManager serializationManager,
            SortedDictionary<string, TValue> source, SortedDictionary<string, TValue> target,
            bool skipHook, ISerializationContext? context = null)
        {
            return _dictionarySerializer.Copy(serializationManager, source, target, skipHook, context);
        }

        public IReadOnlyDictionary<string, TValue> Copy(ISerializationManager serializationManager,
            IReadOnlyDictionary<string, TValue> source,
            IReadOnlyDictionary<string, TValue> target, bool skipHook, ISerializationContext? context = null)
        {
            return _dictionarySerializer.Copy(serializationManager, source, target, skipHook, context);
        }
    }
}
