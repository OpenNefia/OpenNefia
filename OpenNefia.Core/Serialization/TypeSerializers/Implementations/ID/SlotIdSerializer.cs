using System.Globalization;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.GameObjects;
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
    public class SlotIdSerializer : ITypeSerializer<SlotId, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var val = int.Parse(node.Value, CultureInfo.InvariantCulture);
            return new DeserializedValue<SlotId>(new SlotId(val));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return int.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing SlotId");
        }

        public DataNode Write(ISerializationManager serializationManager, SlotId value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var val = (int)value;
            return new ValueDataNode(val.ToString());
        }

        [MustUseReturnValue]
        public SlotId Copy(ISerializationManager serializationManager, SlotId source, SlotId target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.Value);
        }

        public bool Compare(ISerializationManager serializationManager, SlotId left, SlotId right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
