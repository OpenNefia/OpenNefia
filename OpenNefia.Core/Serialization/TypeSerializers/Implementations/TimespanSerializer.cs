using System;
using System.Globalization;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class TimeSpanSerializer : ITypeSerializer<TimeSpan, ValueDataNode>
    {
        public TimeSpan Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null, TimeSpan value = default)
        {
            var seconds = double.Parse(node.Value, CultureInfo.InvariantCulture);
            return TimeSpan.FromSeconds(seconds);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return double.TryParse(node.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing TimeSpan");
        }

        public DataNode Write(ISerializationManager serializationManager, TimeSpan value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
        }

        [MustUseReturnValue]
        public TimeSpan Copy(ISerializationManager serializationManager, TimeSpan source, TimeSpan target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, TimeSpan left, TimeSpan right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
