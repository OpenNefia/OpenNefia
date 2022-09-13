using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown.Mapping;

namespace OpenNefia.Core.Serialization.Manager.Definition
{
    public partial class DataDefinition
    {
        private delegate DeserializedFieldEntry[] DeserializeDelegate(
            MappingDataNode mappingDataNode,
            ISerializationManager serializationManager,
            ISerializationContext? context,
            bool skipHook);

        private delegate object PopulateDelegateSignature(
            object target,
            MappingDataNode mappingDataNode,
            ISerializationManager serializationManager,
            ISerializationContext? context,
            bool skipHook,
            object?[] defaultValues);

        private delegate MappingDataNode SerializeDelegateSignature(
            object obj,
            ISerializationManager serializationManager,
            ISerializationContext? context,
            bool alwaysWrite,
            object?[] defaultValues);

        private delegate object CopyDelegateSignature(
            object source,
            object target,
            ISerializationManager serializationManager,
            ISerializationContext? context);

        private delegate bool CompareDelegateSignature(
            object objA,
            object objB,
            ISerializationManager serializationManager,
            ISerializationContext? context);

        private delegate DeserializationResult CreateDefinitionDelegate(
            object value,
            DeserializedFieldEntry[] mappings);

        private delegate TValue AccessField<TTarget, TValue>(ref TTarget target);

        internal delegate void AssignField<TTarget, TValue>(ref TTarget target, TValue? value);
    }
}
