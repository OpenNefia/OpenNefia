using System;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations.Custom
{
    public class ConstantSerializer<TTag> : ITypeSerializer<int, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var constType = serializationManager.GetConstantTypeFromTag(typeof(TTag));
            return Enum.TryParse(constType, node.Value, out _) ? new ValidatedValueNode(node) : new ErrorNode(node, "Failed parsing constant.", false);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            var constType = serializationManager.GetConstantTypeFromTag(typeof(TTag));
            return new DeserializedValue((int) Enum.Parse(constType, node.Value));
        }

        public DataNode Write(ISerializationManager serializationManager, int value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var constType = serializationManager.GetConstantTypeFromTag(typeof(TTag));
            var constantName = Enum.GetName(constType, value);

            if (constantName == null)
            {
                throw new InvalidOperationException($"No constant corresponding to value {value} in {constType}.");
            }

            return new ValueDataNode(constantName);
        }

        public int Copy(ISerializationManager serializationManager, int source, int target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }
    }
}
