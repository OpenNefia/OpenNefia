using System;
using System.Globalization;
using JetBrains.Annotations;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class TimespanSerializer : ITypeSerializer<TimeSpan, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var seconds = double.Parse(node.Value, CultureInfo.InvariantCulture);
            return new DeserializedValue<TimeSpan>(TimeSpan.FromSeconds(seconds));
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
    }
}
