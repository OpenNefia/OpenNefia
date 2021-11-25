using JetBrains.Annotations;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Markdown.Validation;

namespace Why.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeValidator<[UsedImplicitly]TType, TNode>
    {
        ValidationNode Validate(
            ISerializationManager serializationManager,
            TNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null);
    }
}
