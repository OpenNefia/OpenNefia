using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class LocaleKeySerializer : ITypeSerializer<LocaleKey, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new DeserializedValue<LocaleKey>(new LocaleKey(node.Value));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, LocaleKey value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.Key);
        }

        [MustUseReturnValue]
        public LocaleKey Copy(ISerializationManager serializationManager, LocaleKey source, LocaleKey target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.Key);
        }

        public bool Compare(ISerializationManager serializationManager, LocaleKey left, LocaleKey right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
