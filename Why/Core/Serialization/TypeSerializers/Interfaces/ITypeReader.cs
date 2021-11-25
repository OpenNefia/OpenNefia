using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;

namespace Why.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeReader<TType, TNode> : ITypeValidator<TType, TNode> where TNode : DataNode
    {
        DeserializationResult Read(
            ISerializationManager serializationManager,
            TNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null);
    }
}
