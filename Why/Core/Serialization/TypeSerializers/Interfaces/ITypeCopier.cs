using JetBrains.Annotations;
using Why.Core.Serialization.Manager;

namespace Why.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeCopier<TType>
    {
        [MustUseReturnValue]
        TType Copy(ISerializationManager serializationManager, TType source, TType target,
            bool skipHook,
            ISerializationContext? context = null);
    }
}
