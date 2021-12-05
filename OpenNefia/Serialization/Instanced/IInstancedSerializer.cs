using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Serialization.Instanced
{
    /// <summary>
    /// This serializer is intended for game saves, and uses a binary format.
    /// In contrast, <see cref="Manager.ISerializationManager"/> is used for
    /// user-facing deserialization of YAML-formatted data.
    /// </summary>
    public interface IInstancedSerializer
    {
        public T DeserializeValue<T>(Stream stream);
        public T DeserializeValue<T>(byte[] content);
        public void SerializeValue<T>(T value, Stream stream);
    }
}
