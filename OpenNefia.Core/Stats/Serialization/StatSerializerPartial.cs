using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;

namespace OpenNefia.Core.Stats.Serialization
{
    /// <summary>
    /// Default serializer for <see cref="Stat{T}"/>, which only saves the base value of the stat.
    /// </summary>
    [TypeSerializer]
    public sealed class StatSerializerPartial<T> : BaseStatSerializer<T>
    {
        public override DataNode Write(ISerializationManager serializationManager, Stat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.WriteValue(typeof(T), value.Base, alwaysWrite, context);
        }
    }
}
