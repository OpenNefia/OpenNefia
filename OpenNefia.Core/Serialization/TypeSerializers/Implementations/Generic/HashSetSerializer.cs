using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic
{
    [TypeSerializer]
    public class HashSetSerializer<T> :
        ITypeSerializer<HashSet<T>, SequenceDataNode>,
        ITypeSerializer<ImmutableHashSet<T>, SequenceDataNode>,
        ITypeSerializer<SortedSet<T>, SequenceDataNode>,
        ITypeSerializer<ImmutableSortedSet<T>, SequenceDataNode>
    {
        DeserializationResult ITypeReader<HashSet<T>, SequenceDataNode>.Read(ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context)
        {
            var set = new HashSet<T>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var (value, result) = serializationManager.ReadWithValueOrThrow<T>(dataNode, context, skipHook);

                set.Add(value);
                mappings.Add(result);
            }

            return new DeserializedCollection<HashSet<T>, T>(set, mappings, elements => new HashSet<T>(elements));
        }

        DeserializationResult ITypeReader<ImmutableHashSet<T>, SequenceDataNode>.Read(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context)
        {
            var set = ImmutableHashSet.CreateBuilder<T>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var (value, result) = serializationManager.ReadWithValueOrThrow<T>(dataNode, context, skipHook);

                set.Add(value);
                mappings.Add(result);
            }

            return new DeserializedCollection<ImmutableHashSet<T>, T>(set.ToImmutable(), mappings, elements => ImmutableHashSet.Create(elements.ToArray()));
        }

        DeserializationResult ITypeReader<SortedSet<T>, SequenceDataNode>.Read(ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context)
        {
            var set = new SortedSet<T>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var (value, result) = serializationManager.ReadWithValueOrThrow<T>(dataNode, context, skipHook);

                set.Add(value);
                mappings.Add(result);
            }

            return new DeserializedCollection<SortedSet<T>, T>(set, mappings, elements => new SortedSet<T>(elements));
        }

        DeserializationResult ITypeReader<ImmutableSortedSet<T>, SequenceDataNode>.Read(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context)
        {
            var set = ImmutableSortedSet.CreateBuilder<T>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var (value, result) = serializationManager.ReadWithValueOrThrow<T>(dataNode, context, skipHook);

                set.Add(value);
                mappings.Add(result);
            }

            return new DeserializedCollection<ImmutableSortedSet<T>, T>(set.ToImmutable(), mappings, elements => ImmutableSortedSet.Create(elements.ToArray()));
        }

        ValidationNode ITypeValidator<ImmutableHashSet<T>, SequenceDataNode>.Validate(
            ISerializationManager serializationManager,
            SequenceDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, context);
        }

        ValidationNode ITypeValidator<HashSet<T>, SequenceDataNode>.Validate(ISerializationManager serializationManager,
            SequenceDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, context);
        }

        ValidationNode ITypeValidator<ImmutableSortedSet<T>, SequenceDataNode>.Validate(
            ISerializationManager serializationManager,
            SequenceDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, context);
        }

        ValidationNode ITypeValidator<SortedSet<T>, SequenceDataNode>.Validate(ISerializationManager serializationManager,
            SequenceDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            return Validate(serializationManager, node, context);
        }

        ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node, ISerializationContext? context)
        {
            var list = new List<ValidationNode>();
            foreach (var elem in node.Sequence)
            {
                list.Add(serializationManager.ValidateNode(typeof(T), elem, context));
            }

            return new ValidatedSequenceNode(list);
        }

        public DataNode Write(ISerializationManager serializationManager, ImmutableHashSet<T> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return Write(serializationManager, value.ToHashSet(), alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, HashSet<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var sequence = new SequenceDataNode();

            foreach (var elem in value)
            {
                sequence.Add(serializationManager.WriteValue(typeof(T), elem, alwaysWrite, context));
            }

            return sequence;
        }

        public DataNode Write(ISerializationManager serializationManager, ImmutableSortedSet<T> value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return Write(serializationManager, new SortedSet<T>(value), alwaysWrite, context);
        }

        public DataNode Write(ISerializationManager serializationManager, SortedSet<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var sequence = new SequenceDataNode();

            foreach (var elem in value)
            {
                sequence.Add(serializationManager.WriteValue(typeof(T), elem, alwaysWrite, context));
            }

            return sequence;
        }

        [MustUseReturnValue]
        public HashSet<T> Copy(ISerializationManager serializationManager, HashSet<T> source, HashSet<T> target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            target.Clear();
            target.EnsureCapacity(source.Count);

            foreach (var element in source)
            {
                var elementCopy = serializationManager.CreateCopy(element, context) ?? throw new NullReferenceException();
                target.Add(elementCopy);
            }

            return target;
        }

        [MustUseReturnValue]
        public ImmutableHashSet<T> Copy(ISerializationManager serializationManager, ImmutableHashSet<T> source,
            ImmutableHashSet<T> target, bool skipHook, ISerializationContext? context = null)
        {
            var builder = ImmutableHashSet.CreateBuilder<T>();

            foreach (var element in source)
            {
                var elementCopy = serializationManager.CreateCopy(element, context) ?? throw new NullReferenceException();
                builder.Add(elementCopy);
            }

            return builder.ToImmutable();
        }

        [MustUseReturnValue]
        public SortedSet<T> Copy(ISerializationManager serializationManager, SortedSet<T> source, SortedSet<T> target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            target.Clear();

            foreach (var element in source)
            {
                var elementCopy = serializationManager.CreateCopy(element, context) ?? throw new NullReferenceException();
                target.Add(elementCopy);
            }

            return target;
        }

        [MustUseReturnValue]
        public ImmutableSortedSet<T> Copy(ISerializationManager serializationManager, ImmutableSortedSet<T> source,
            ImmutableSortedSet<T> target, bool skipHook, ISerializationContext? context = null)
        {
            var builder = ImmutableSortedSet.CreateBuilder<T>();

            foreach (var element in source)
            {
                var elementCopy = serializationManager.CreateCopy(element, context) ?? throw new NullReferenceException();
                builder.Add(elementCopy);
            }

            return builder.ToImmutable();
        }

        public bool Compare(ISerializationManager serializationManager, HashSet<T> left, HashSet<T> right,
            bool skipHook, 
            ISerializationContext? context = null)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var elem in left)
            {
                // TODO: is this correct? it will bypass the serialization manager for complex types.
                if (!right.Contains(elem))
                    return false;
            }

            return true;
        }

        public bool Compare(ISerializationManager serializationManager, ImmutableHashSet<T> left, ImmutableHashSet<T> right, 
            bool skipHook, 
            ISerializationContext? context = null)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var elem in left)
            {
                // TODO: is this correct? it will bypass the serialization manager for complex types.
                if (!right.Contains(elem))
                    return false;
            }

            return true;
        }

        public bool Compare(ISerializationManager serializationManager, SortedSet<T> left, SortedSet<T> right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var elem in left)
            {
                // TODO: is this correct? it will bypass the serialization manager for complex types.
                if (!right.Contains(elem))
                    return false;
            }

            return true;
        }

        public bool Compare(ISerializationManager serializationManager, ImmutableSortedSet<T> left, ImmutableSortedSet<T> right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var elem in left)
            {
                // TODO: is this correct? it will bypass the serialization manager for complex types.
                if (!right.Contains(elem))
                    return false;
            }

            return true;
        }
    }
}
