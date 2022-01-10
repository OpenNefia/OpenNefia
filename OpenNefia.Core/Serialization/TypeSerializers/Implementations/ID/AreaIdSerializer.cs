using System.Globalization;
using JetBrains.Annotations;
using OpenNefia.Core.Areas;
using OpenNefia.Core.IoC;
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
    public class AreaIdSerializer : ITypeSerializer<AreaId, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var val = int.Parse(node.Value, CultureInfo.InvariantCulture);
            return new DeserializedValue<AreaId>(new AreaId(val));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return int.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing AreaId");
        }

        public DataNode Write(ISerializationManager serializationManager, AreaId value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var val = (int)value;
            return new ValueDataNode(val.ToString());
        }

        [MustUseReturnValue]
        public AreaId Copy(ISerializationManager serializationManager, AreaId source, AreaId target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.Value);
        }

        public bool Compare(ISerializationManager serializationManager, AreaId left, AreaId right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
