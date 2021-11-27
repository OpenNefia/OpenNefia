using System.Collections.Generic;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary
{
    // [TypeSerializer]
    public class PrototypeIdDictionarySerializer<TPrototype, TValue> :
        ITypeSerializer<Dictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>,
        ITypeSerializer<SortedDictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>,
        ITypeSerializer<IReadOnlyDictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>
        where TPrototype : class, IPrototype
    {
        private readonly DictionarySerializer<PrototypeId<TPrototype>, TValue> _dictionarySerializer = new();
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

        ValidationNode ITypeValidator<Dictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, dependencies, context);
        }

        ValidationNode ITypeValidator<SortedDictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, dependencies, context);
        }

        ValidationNode ITypeValidator<IReadOnlyDictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, dependencies, context);
        }

        DeserializationResult ITypeReader<Dictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>.Read(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context)
        {
            return _dictionarySerializer.Read(serializationManager, node, dependencies, skipHook, context);
        }

        DeserializationResult ITypeReader<SortedDictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>.Read(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context)
        {
            return _dictionarySerializer.Read(serializationManager, node, dependencies, skipHook, context);
        }

        DeserializationResult ITypeReader<IReadOnlyDictionary<PrototypeId<TPrototype>, TValue>, MappingDataNode>.Read(
            ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context)
        {
            return _dictionarySerializer.Read(serializationManager, node, dependencies, skipHook, context);
        }

        public DataNode Write(ISerializationManager serializationManager, Dictionary<PrototypeId<TPrototype>, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Write(serializationManager, value, alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, SortedDictionary<PrototypeId<TPrototype>, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Write(serializationManager, value, alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, IReadOnlyDictionary<PrototypeId<TPrototype>, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Write(serializationManager, value, alwaysWrite, context);
        }

        public Dictionary<PrototypeId<TPrototype>, TValue> Copy(ISerializationManager serializationManager,
            Dictionary<PrototypeId<TPrototype>, TValue> source, Dictionary<PrototypeId<TPrototype>, TValue> target, bool skipHook,
            ISerializationContext? context = null)
        {
            return _dictionarySerializer.Copy(serializationManager, source, target, skipHook, context);
        }

        public SortedDictionary<PrototypeId<TPrototype>, TValue> Copy(ISerializationManager serializationManager,
            SortedDictionary<PrototypeId<TPrototype>, TValue> source, SortedDictionary<PrototypeId<TPrototype>, TValue> target,
            bool skipHook, ISerializationContext? context = null)
        {
            return _dictionarySerializer.Copy(serializationManager, source, target, skipHook, context);
        }

        public IReadOnlyDictionary<PrototypeId<TPrototype>, TValue> Copy(ISerializationManager serializationManager,
            IReadOnlyDictionary<PrototypeId<TPrototype>, TValue> source,
            IReadOnlyDictionary<PrototypeId<TPrototype>, TValue> target, bool skipHook, ISerializationContext? context = null)
        {
            return _dictionarySerializer.Copy(serializationManager, source, target, skipHook, context);
        }
    }
}
