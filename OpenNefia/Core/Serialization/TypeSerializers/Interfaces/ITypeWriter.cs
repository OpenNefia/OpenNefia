using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;

namespace OpenNefia.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeWriter<TType>
    {
        DataNode Write(ISerializationManager serializationManager, TType value, bool alwaysWrite = false,
            ISerializationContext? context = null);
    }
}
