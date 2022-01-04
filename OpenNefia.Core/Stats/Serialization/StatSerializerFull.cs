using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;

namespace OpenNefia.Core.Stats.Serialization
{
    /// <summary>
    /// This serializer for <see cref="Stat{T}"/> properties will save both the base and buffed value.
    /// Use this if it's important to keep both for some reason. However, note that the buffed value is 
    /// meant to be wiped and recalculated when a save is loaded, so usually there's no point (it will
    /// only increase the save size for every stat property saved).
    /// </summary>
    public sealed class StatSerializerFull<T> : BaseStatSerializer<T>
    {
        public override DataNode Write(ISerializationManager serializationManager, Stat<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var mapping = new MappingDataNode();

            mapping.Add("base", serializationManager.WriteValue(typeof(T), value.Base, alwaysWrite, context));
            mapping.Add("buffed", serializationManager.WriteValue(typeof(T), value.Buffed, alwaysWrite, context));

            return mapping;
        }
    }
}
