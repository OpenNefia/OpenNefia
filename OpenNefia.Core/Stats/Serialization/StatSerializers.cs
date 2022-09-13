using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
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
        public Stat<T> Read(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            Stat<T>? rawValue = null)
        {
            if (!node.TryGet("base", out var baseNode))
                throw new InvalidMappingException($"No 'base' mapping provided to {nameof(StatSerializerFull<T>)}");
            if (!node.TryGet("buffed", out var buffedNode))
                throw new InvalidMappingException($"No 'buffed' mapping provided to {nameof(StatSerializerFull<T>)}");

            var baseValue = serializationManager.Read<T>(baseNode, context, skipHook);
            var buffedValue = serializationManager.Read<T>(buffedNode, context, skipHook);

            return new Stat<T>(baseValue, buffedValue);
        }

        public Stat<T> Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            Stat<T>? rawValue = null)
        {
            var baseValue = serializationManager.Read<T>(node, context, skipHook);

            return new Stat<T>(baseValue);
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
            var targetBase = target.Base;
            var targetBuffed = target.Buffed;
            serializationManager.Copy(source.Base, ref targetBase, context, skipHook);
            serializationManager.Copy(source.Buffed, ref targetBuffed, context, skipHook);
            return new(targetBase, targetBuffed);
        }

        public bool Compare(ISerializationManager serializationManager, Stat<T> left, Stat<T> right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return serializationManager.Compare(left.Base, right.Base, context, skipHook)
                && serializationManager.Compare(left.Buffed, right.Buffed, context, skipHook);
        }
    }
    
    /// <summary>
    /// This serializer for <see cref="Stat{T}"/> properties will save both the base and buffed value.
    /// Use this if it's important to keep both for some reason. However, note that the buffed value is 
    /// meant to be wiped and recalculated when a save is loaded, so usually there's no point (it will
    /// only increase the save size for every stat property saved).
    /// </summary>
    public sealed class StatSerializerFull<T> : BaseStatSerializer<T>
    {
        public override DataNode Write(ISerializationManager serializationManager, Stat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var mapping = new MappingDataNode();

            mapping.Add("base", serializationManager.WriteValue(typeof(T), value.Base, alwaysWrite, context));
            mapping.Add("buffed", serializationManager.WriteValue(typeof(T), value.Buffed, alwaysWrite, context));

            return mapping;
        }
    }
    
    /// <summary>
    /// Default serializer for <see cref="Stat{T}"/>, which only saves the base value of the stat.
    /// </summary>
    [TypeSerializer]
    public sealed class StatSerializerPartial<T> : BaseStatSerializer<T>
    {
        public override DataNode Write(ISerializationManager serializationManager, Stat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.WriteValue(typeof(T), value.Base, alwaysWrite, context);
        }
    }
}
