using OpenNefia.Core.GameObjects;
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
    public class EntitySerializer : ITypeReaderWriter<Entity, ValueDataNode>
    {
        public DeserializationResult Read(
            ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null)
        {
            var uid = new EntityUid(int.Parse(node.Value));

            if (!uid.IsValid())
            {
                throw new InvalidMappingException($"{node.Value} is not a valid entity uid.");
            }

            var entity = dependencies.Resolve<IEntityManager>().GetEntity(uid);
            return new DeserializedValue<Entity>(entity);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var uid = new EntityUid(int.Parse(node.Value));
            return uid.IsValid() &&
                   dependencies.Resolve<IEntityManager>().EntityExists(uid)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing EntityUid");
        }

        public DataNode Write(ISerializationManager serializationManager, Entity value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.Uid.ToString());
        }
    }
}
