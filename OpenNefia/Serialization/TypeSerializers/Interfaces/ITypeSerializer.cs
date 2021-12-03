using OpenNefia.Core.Serialization.Markdown;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeSerializer<TType, TNode> :
        ITypeReaderWriter<TType, TNode>,
        ITypeCopier<TType>
        where TNode : DataNode
    {
    }
}
