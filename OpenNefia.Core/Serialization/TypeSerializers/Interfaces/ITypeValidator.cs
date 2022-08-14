using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Validation;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeValidator<[UsedImplicitly] TType, TNode>
    {
        ValidationNode Validate(
            ISerializationManager serializationManager,
            TNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null);
    }
}
