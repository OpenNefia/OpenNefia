using OpenNefia.Content.Dialog;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Content.Skills
{
    [TypeSerializer]
    public class DialogTextEntrySerializer : ITypeSerializer<DialogTextEntry, ValueDataNode>
    {
        public DeserializationResult Read(
            ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null)
        {
            var result = DialogTextEntry.FromLocaleKey(node.Value);
            return new DeserializedValue<DialogTextEntry>(result);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, DialogTextEntry value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.GetDefinition<DialogTextEntry>()!
                .Serialize(value, serializationManager, context, alwaysWrite);
        }

        public DialogTextEntry Copy(ISerializationManager serializationManager, DialogTextEntry source, DialogTextEntry target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            target.Text = source.Text;
            target.Key = source.Key;
            return target;
        }

        public bool Compare(ISerializationManager serializationManager, DialogTextEntry left, DialogTextEntry right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left.Text == right.Text && left.Key == right.Key;
        }
    }
}
