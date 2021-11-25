using Why.Core.Serialization.Markdown;

namespace Why.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeReaderWriter<TType, TNode> :
        ITypeReader<TType, TNode>,
        ITypeWriter<TType>
        where TNode : DataNode
    {
    }
}
