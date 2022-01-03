using System.Globalization;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class VersionSerializer : ITypeSerializer<Version, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new DeserializedValue<Version>(Version.Parse(node.Value));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return Version.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing values for {nameof(Version)}.");
        }

        public DataNode Write(ISerializationManager serializationManager, Version value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString());
        }

        public Version Copy(ISerializationManager serializationManager, Version source, Version target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return (Version)source.Clone();
        }

        public bool Compare(ISerializationManager serializationManager, Version left, Version right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
