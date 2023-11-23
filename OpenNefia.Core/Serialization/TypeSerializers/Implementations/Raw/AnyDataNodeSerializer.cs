using System.Globalization;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class AnyDataNodeSerializer : ITypeSerializer<AnyDataNode, ValueDataNode>,
        ITypeSerializer<AnyDataNode, SequenceDataNode>, 
        ITypeSerializer<AnyDataNode, MappingDataNode>
    {
        public AnyDataNode Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            AnyDataNode? rawValue = null)
        {
            return new(node);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }
        
        public AnyDataNode Read(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            AnyDataNode? rawValue = null)
        {
            return new(node);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }

        public AnyDataNode Read(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            AnyDataNode? rawValue = null)
        {
            return new(node);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, AnyDataNode value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return value.Node;
        }

        public AnyDataNode Copy(ISerializationManager serializationManager, AnyDataNode source, AnyDataNode target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return source.Copy();
        }

        public bool Compare(ISerializationManager serializationManager, AnyDataNode left, AnyDataNode right,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (left.GetType() != right.GetType())
                return false;
            return true;
        }
    }
}
