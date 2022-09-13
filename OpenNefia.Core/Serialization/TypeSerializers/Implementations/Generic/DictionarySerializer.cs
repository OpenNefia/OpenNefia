using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic
{
    [TypeSerializer]
    public class DictionarySerializer<TKey, TValue> :
        ITypeSerializer<Dictionary<TKey, TValue>, MappingDataNode>,
        ITypeSerializer<IReadOnlyDictionary<TKey, TValue>, MappingDataNode>,
        ITypeSerializer<SortedDictionary<TKey, TValue>, MappingDataNode> where TKey : notnull
    {
        private MappingDataNode InterfaceWrite(
            ISerializationManager serializationManager,
            IDictionary<TKey, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var mappingNode = new MappingDataNode();

            foreach (var (key, val) in value)
            {
                mappingNode.Add(
                    serializationManager.WriteValue(key, alwaysWrite, context),
                    serializationManager.WriteValue(typeof(TValue), val, alwaysWrite, context));
            }

            return mappingNode;
        }

        public Dictionary<TKey, TValue> Read(ISerializationManager serializationManager,
            MappingDataNode node, IDependencyCollection dependencies, bool skipHook, ISerializationContext? context,
            Dictionary<TKey, TValue>? dict)
        {
            dict ??= new Dictionary<TKey, TValue>();

            foreach (var (key, value) in node.Children)
            {
                dict.Add(serializationManager.Read<TKey>(key, context, skipHook),
                    serializationManager.Read<TValue>(value, context, skipHook));
            }

            return dict;
        }

        ValidationNode ITypeValidator<SortedDictionary<TKey, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node, IDependencyCollection dependencies,
            ISerializationContext? context)
        {
            return Validate(serializationManager, node, context);
        }

        ValidationNode ITypeValidator<IReadOnlyDictionary<TKey, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager, MappingDataNode node, IDependencyCollection dependencies,
            ISerializationContext? context)
        {
            return Validate(serializationManager, node, context);
        }

        ValidationNode ITypeValidator<Dictionary<TKey, TValue>, MappingDataNode>.Validate(
            ISerializationManager serializationManager,
            MappingDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, context);
        }

        ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node, ISerializationContext? context)
        {
            var mapping = new Dictionary<ValidationNode, ValidationNode>();
            foreach (var (key, val) in node.Children)
            {
                mapping.Add(serializationManager.ValidateNode(typeof(TKey), key, context), serializationManager.ValidateNode(typeof(TValue), val, context));
            }

            return new ValidatedMappingNode(mapping);
        }

        public DataNode Write(ISerializationManager serializationManager, Dictionary<TKey, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return InterfaceWrite(serializationManager, value, alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, SortedDictionary<TKey, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return InterfaceWrite(serializationManager, value, alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, IReadOnlyDictionary<TKey, TValue> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return InterfaceWrite(serializationManager, value.ToDictionary(k => k.Key, v => v.Value), alwaysWrite, context);
        }



        IReadOnlyDictionary<TKey, TValue>
            ITypeReader<IReadOnlyDictionary<TKey, TValue>, MappingDataNode>.Read(
                ISerializationManager serializationManager, MappingDataNode node,
                IDependencyCollection dependencies,
                bool skipHook, ISerializationContext? context, IReadOnlyDictionary<TKey, TValue>? rawValue)
        {
            if (rawValue != null)
                Logger.Warning($"Provided value to a Read-call for a {nameof(IReadOnlyDictionary<TKey, TValue>)}. Ignoring...");

            var dict = new Dictionary<TKey, TValue>();

            foreach (var (key, value) in node.Children)
            {
                dict.Add(serializationManager.Read<TKey>(key, context, skipHook), serializationManager.Read<TValue>(value, context, skipHook));
            }

            return dict;
        }

        SortedDictionary<TKey, TValue>
            ITypeReader<SortedDictionary<TKey, TValue>, MappingDataNode>.Read(
                ISerializationManager serializationManager, MappingDataNode node,
                IDependencyCollection dependencies,
                bool skipHook, ISerializationContext? context, SortedDictionary<TKey, TValue>? dict)
        {
            dict ??= new SortedDictionary<TKey, TValue>();

            foreach (var (key, value) in node.Children)
            {
                dict.Add(serializationManager.Read<TKey>(key, context, skipHook), serializationManager.Read<TValue>(value, context, skipHook));
            }

            return dict;
        }

        [MustUseReturnValue]
        private T CopyInternal<T>(ISerializationManager serializationManager, IReadOnlyDictionary<TKey, TValue> source, T target, ISerializationContext? context) where T : IDictionary<TKey, TValue>
        {
            target.Clear();

            foreach (var (key, value) in source)
            {
                var keyCopy = serializationManager.Copy(key, context) ?? throw new NullReferenceException();
                var valueCopy = serializationManager.Copy(value, context)!;

                target.Add(keyCopy, valueCopy);
            }

            return target;
        }

        [MustUseReturnValue]
        public Dictionary<TKey, TValue> Copy(ISerializationManager serializationManager,
            Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> target,
            bool skipHook, ISerializationContext? context = null)
        {
            return CopyInternal(serializationManager, source, target, context);
        }

        [MustUseReturnValue]
        public IReadOnlyDictionary<TKey, TValue> Copy(ISerializationManager serializationManager,
            IReadOnlyDictionary<TKey, TValue> source, IReadOnlyDictionary<TKey, TValue> target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (target is Dictionary<TKey, TValue> targetDictionary)
            {
                return CopyInternal(serializationManager, source, targetDictionary, context);
            }

            var dictionary = new Dictionary<TKey, TValue>(source.Count);

            foreach (var (key, value) in source)
            {
                var keyCopy = serializationManager.Copy(key, context) ?? throw new NullReferenceException();
                var valueCopy = serializationManager.Copy(value, context)!;

                dictionary.Add(keyCopy, valueCopy);
            }

            return dictionary;
        }

        [MustUseReturnValue]
        public SortedDictionary<TKey, TValue> Copy(ISerializationManager serializationManager,
            SortedDictionary<TKey, TValue> source, SortedDictionary<TKey, TValue> target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return CopyInternal(serializationManager, source, target, context);
        }

        public bool CompareInternal(ISerializationManager serializationManager, IReadOnlyDictionary<TKey, TValue> left, IReadOnlyDictionary<TKey, TValue> right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var (key, valueLeft) in left)
            {
                if (!right.TryGetValue(key, out var compRight))
                    return false;

                if (!serializationManager.Compare(valueLeft, compRight))
                    return false;
            }

            return true;
        }

        public bool Compare(ISerializationManager serializationManager, Dictionary<TKey, TValue> left, Dictionary<TKey, TValue> right, 
            bool skipHook, 
            ISerializationContext? context = null)
        {
            return CompareInternal(serializationManager, left, right, skipHook, context);
        }

        public bool Compare(ISerializationManager serializationManager, IReadOnlyDictionary<TKey, TValue> left, IReadOnlyDictionary<TKey, TValue> right, 
            bool skipHook, 
            ISerializationContext? context = null)
        {
            return CompareInternal(serializationManager, left, right, skipHook, context);
        }

        public bool Compare(ISerializationManager serializationManager, SortedDictionary<TKey, TValue> left, SortedDictionary<TKey, TValue> right, 
            bool skipHook, 
            ISerializationContext? context = null)
        {
            return CompareInternal(serializationManager, left, right, skipHook, context);
        }
    }
}
