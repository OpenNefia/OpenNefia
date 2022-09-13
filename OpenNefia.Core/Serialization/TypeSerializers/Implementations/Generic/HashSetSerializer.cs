using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
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
        HashSet<T> ITypeReader<HashSet<T>, SequenceDataNode>.Read(ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context, HashSet<T>? set)
        {
            set ??= new HashSet<T>();

            foreach (var dataNode in node.Sequence)
            {
                set.Add(serializationManager.Read<T>(dataNode, context, skipHook));
            }

            return set;
        }

        ImmutableHashSet<T> ITypeReader<ImmutableHashSet<T>, SequenceDataNode>.Read(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context, ImmutableHashSet<T>? rawValue)
        {
            if (rawValue != null)
                Logger.Warning($"Provided value to a Read-call for a {nameof(ImmutableHashSet<T>)}. Ignoring...");
            var set = ImmutableHashSet.CreateBuilder<T>();

            foreach (var dataNode in node.Sequence)
            {
                set.Add(serializationManager.Read<T>(dataNode, context, skipHook));
            }

            return set.ToImmutable();
        }

        SortedSet<T> ITypeReader<SortedSet<T>, SequenceDataNode>.Read(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context, SortedSet<T>? rawValue)
        {
            if (rawValue != null)
                Logger.Warning($"Provided value to a Read-call for a {nameof(ImmutableHashSet<T>)}. Ignoring...");
            var set = new SortedSet<T>();

            foreach (var dataNode in node.Sequence)
            {
                set.Add(serializationManager.Read<T>(dataNode, context, skipHook));
            }

            return set;
        }

        ImmutableSortedSet<T> ITypeReader<ImmutableSortedSet<T>, SequenceDataNode>.Read(
            ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context, ImmutableSortedSet<T>? rawValue)
        {
            if (rawValue != null)
                Logger.Warning($"Provided value to a Read-call for a {nameof(ImmutableHashSet<T>)}. Ignoring...");
            var set = ImmutableSortedSet.CreateBuilder<T>();

            foreach (var dataNode in node.Sequence)
            {
                set.Add(serializationManager.Read<T>(dataNode, context, skipHook));
            }

            return set.ToImmutable();
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
                var elementCopy = serializationManager.Copy(element, context) ?? throw new NullReferenceException();
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
                var elementCopy = serializationManager.Copy(element, context) ?? throw new NullReferenceException();
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
                var elementCopy = serializationManager.Copy(element, context) ?? throw new NullReferenceException();
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
                var elementCopy = serializationManager.Copy(element, context) ?? throw new NullReferenceException();
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
