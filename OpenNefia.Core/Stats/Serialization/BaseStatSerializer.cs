using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Stats.Serialization
{
    public abstract class BaseStatSerializer<T>
        : ITypeSerializer<Stat<T>, MappingDataNode>,
          ITypeSerializer<Stat<T>, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (!node.TryGet("base", out var baseNode))
                throw new InvalidMappingException($"No 'base' mapping provided to {nameof(StatSerializerFull<T>)}");
            if (!node.TryGet("buffed", out var buffedNode))
                throw new InvalidMappingException($"No 'buffed' mapping provided to {nameof(StatSerializerFull<T>)}");

            var baseValue = serializationManager.ReadValueOrThrow<T>(baseNode, context, skipHook);
            var buffedValue = serializationManager.ReadValueOrThrow<T>(buffedNode, context, skipHook);

            return new DeserializedValue<Stat<T>>(new Stat<T>(baseValue, buffedValue));
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var baseValue = serializationManager.ReadValueOrThrow<T>(node, context, skipHook);

            return new DeserializedValue<Stat<T>>(new Stat<T>(baseValue));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            if (!node.TryGet("base", out var baseNode))
                return new ErrorNode(node, $"No 'base' mapping provided to {nameof(StatSerializerFull<T>)}");
            if (!node.TryGet("buffed", out var buffedNode))
                return new ErrorNode(node, $"No 'buffed' mapping provided to {nameof(StatSerializerFull<T>)}");

            var dict = new Dictionary<ValidationNode, ValidationNode>
            {
                {
                    serializationManager.ValidateNode(typeof(T), baseNode, context),
                    serializationManager.ValidateNode(typeof(T), buffedNode, context)
                }
            };

            return new ValidatedMappingNode(dict);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return serializationManager.ValidateNode(typeof(T), node, context);
        }

        public abstract DataNode Write(ISerializationManager serializationManager, Stat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null);

        public Stat<T> Copy(ISerializationManager serializationManager, Stat<T> source, Stat<T> target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(serializationManager.Copy(source.Base, target.Base)!,
                       serializationManager.Copy(source.Buffed, source.Buffed)!);
        }

        public bool Compare(ISerializationManager serializationManager, Stat<T> left, Stat<T> right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return serializationManager.Compare(left.Base, right.Base, context, skipHook)
                && serializationManager.Compare(left.Buffed, right.Buffed, context, skipHook);
        }
    }
}
