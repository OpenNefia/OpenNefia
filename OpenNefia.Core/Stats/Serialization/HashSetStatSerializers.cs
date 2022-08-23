using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Stats.Serialization
{
    public abstract class BaseHashSetStatSerializer<T>
        : ITypeSerializer<HashSetStat<T>, MappingDataNode>,
          ITypeSerializer<HashSetStat<T>, SequenceDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (!node.TryGet("base", out var baseNode))
                throw new InvalidMappingException($"No 'base' mapping provided to {nameof(BaseHashSetStatSerializer<T>)}");
            if (!node.TryGet("buffed", out var buffedNode))
                throw new InvalidMappingException($"No 'buffed' mapping provided to {nameof(BaseHashSetStatSerializer<T>)}");

            var baseValue = serializationManager.ReadValueOrThrow<HashSet<T>>(baseNode, context, skipHook);
            var buffedValue = serializationManager.ReadValueOrThrow<HashSet<T>>(buffedNode, context, skipHook);

            return new DeserializedValue<HashSetStat<T>>(new(baseValue, buffedValue));
        }

        public DeserializationResult Read(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var baseValue = serializationManager.ReadValueOrThrow<HashSet<T>>(node, context, skipHook);

            return new DeserializedValue<HashSetStat<T>>(new HashSetStat<T>(baseValue));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            if (!node.TryGet("base", out var baseNode))
                return new ErrorNode(node, $"No 'base' mapping provided to {nameof(BaseHashSetStatSerializer<T>)}");
            if (!node.TryGet("buffed", out var buffedNode))
                return new ErrorNode(node, $"No 'buffed' mapping provided to {nameof(BaseHashSetStatSerializer<T>)}");

            var dict = new Dictionary<ValidationNode, ValidationNode>
            {
                {
                    serializationManager.ValidateNode(typeof(HashSet<T>), baseNode, context),
                    serializationManager.ValidateNode(typeof(HashSet<T>), buffedNode, context)
                }
            };

            return new ValidatedMappingNode(dict);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return serializationManager.ValidateNode(typeof(HashSet<T>), node, context);
        }

        public abstract DataNode Write(ISerializationManager serializationManager, HashSetStat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null);

        public HashSetStat<T> Copy(ISerializationManager serializationManager, HashSetStat<T> source, HashSetStat<T> target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(serializationManager.Copy(source.Base, target.Base)!,
                       serializationManager.Copy(source.Buffed, source.Buffed)!);
        }

        public bool Compare(ISerializationManager serializationManager, HashSetStat<T> left, HashSetStat<T> right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return serializationManager.Compare(left.Base, right.Base, context, skipHook)
                && serializationManager.Compare(left.Buffed, right.Buffed, context, skipHook);
        }
    }

    public sealed class HashStatSerializerFull<T> : BaseHashSetStatSerializer<T>
    {
        public override DataNode Write(ISerializationManager serializationManager, HashSetStat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var mapping = new MappingDataNode();

            mapping.Add("base", serializationManager.WriteValue(typeof(HashSet<T>), value.Base, alwaysWrite, context));
            mapping.Add("buffed", serializationManager.WriteValue(typeof(HashSet<T>), value.Buffed, alwaysWrite, context));

            return mapping;
        }
    }

    [TypeSerializer]
    public sealed class HashStatSerializerPartial<T> : BaseHashSetStatSerializer<T>
    {
        public override DataNode Write(ISerializationManager serializationManager, HashSetStat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.WriteValue(typeof(HashSet<T>), value.Base, alwaysWrite, context);
        }
    }
}
