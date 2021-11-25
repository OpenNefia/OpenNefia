using Why.Core.GameObjects;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class EntitySerializer : ITypeReaderWriter<IEntity, ValueDataNode>
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
            return new DeserializedValue<IEntity>(entity);
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

        public DataNode Write(ISerializationManager serializationManager, IEntity value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.Uid.ToString());
        }
    }
}
