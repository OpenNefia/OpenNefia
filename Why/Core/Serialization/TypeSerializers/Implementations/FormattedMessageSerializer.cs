using JetBrains.Annotations;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;
using Why.Core.Utility;

namespace Why.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class FormattedMessageSerializer : ITypeSerializer<FormattedMessage, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager,
            ValueDataNode node, IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null)
        {
            return new DeserializedValue<FormattedMessage>(FormattedMessage.FromMarkup(node.Value));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return FormattedMessage.ValidMarkup(node.Value)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Invalid markup in FormattedMessage.");
        }

        public DataNode Write(ISerializationManager serializationManager, FormattedMessage value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToMarkup());
        }

        [MustUseReturnValue]
        public FormattedMessage Copy(ISerializationManager serializationManager, FormattedMessage source,
            FormattedMessage target, bool skipHook, ISerializationContext? context = null)
        {
            return new(source);
        }
    }
}
