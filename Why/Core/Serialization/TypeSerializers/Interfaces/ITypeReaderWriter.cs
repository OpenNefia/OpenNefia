using OpenNefia.Core.Serialization.Markdown;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeReaderWriter<TType, TNode> :
        ITypeReader<TType, TNode>,
        ITypeWriter<TType>
        where TNode : DataNode
    {
    }
}
