using Why.Core.Serialization.Markdown;

namespace Why.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeSerializer<TType, TNode> :
        ITypeReaderWriter<TType, TNode>,
        ITypeCopier<TType>
        where TNode : DataNode
    {
    }
}
