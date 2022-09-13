using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeReader<TType, TNode> : ITypeValidator<TType, TNode> where TNode : DataNode
    {
        TType Read(ISerializationManager serializationManager,
            TNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null, TType? value = default);
    }
}
