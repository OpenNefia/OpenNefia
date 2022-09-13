using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeInheritanceHandler<[UsedImplicitly] TType, TNode> where TNode : DataNode
    {
        TNode PushInheritance(ISerializationManager serializationManager, TNode child, TNode parent,
            IDependencyCollection dependencies, ISerializationContext context);
    }
}