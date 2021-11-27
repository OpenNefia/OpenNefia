using JetBrains.Annotations;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeCopier<TType>
    {
        [MustUseReturnValue]
        TType Copy(ISerializationManager serializationManager, TType source, TType target,
            bool skipHook,
            ISerializationContext? context = null);
    }
}
