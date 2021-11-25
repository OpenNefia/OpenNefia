using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Markdown;

namespace Why.Core.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeWriter<TType>
    {
        DataNode Write(ISerializationManager serializationManager, TType value, bool alwaysWrite = false,
            ISerializationContext? context = null);
    }
}
