using JetBrains.Annotations;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeComparer<TType>
    {
        bool Compare(ISerializationManager serializationManager, TType objA, TType objB,
            bool skipHook,
            ISerializationContext? context = null);
    }
}
