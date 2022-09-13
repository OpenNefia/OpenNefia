using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class FormattedMessageSerializer : ITypeSerializer<FormattedMessage, ValueDataNode>
    {
        public FormattedMessage Read(ISerializationManager serializationManager,
            ValueDataNode node, IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null, FormattedMessage? value = default)
        {
            return FormattedMessage.FromMarkup(node.Value);
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

        public bool Compare(ISerializationManager serializationManager, FormattedMessage left, FormattedMessage right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
