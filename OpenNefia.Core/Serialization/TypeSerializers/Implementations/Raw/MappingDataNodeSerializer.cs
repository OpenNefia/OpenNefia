using System.Globalization;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class MappingDataNodeSerializer : ITypeSerializer<MappingDataNode, MappingDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new DeserializedValue<MappingDataNode>(node);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, MappingDataNode value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return value;
        }

        public MappingDataNode Copy(ISerializationManager serializationManager, MappingDataNode source, MappingDataNode target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            target.Clear();
            foreach (var (key, val) in source.Children)
            {
                target.Add(key.Copy(), val.Copy());
            }
            return target;
        }
    }
}
