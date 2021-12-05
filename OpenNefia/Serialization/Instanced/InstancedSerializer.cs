using OdinSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Serialization.Instanced
{
    public class InstancedSerializer : IInstancedSerializer
    {
        SerializationConfig _serConfig;

        public InstancedSerializer()
        {
            _serConfig = new SerializationConfig()
            {
                DebugContext = new DebugContext()
                {
                    Logger = new InstancedSerializerLogger()
                },
                SerializationPolicy = new InstancedSerializationPolicy()
            };
        }

        public T DeserializeValue<T>(Stream stream)
        {
            var deserContext = new DeserializationContext()
            {
                Config = _serConfig,
            };

            return SerializationUtility.DeserializeValue<T>(stream, DataFormat.Binary, deserContext);
        }

        public T DeserializeValue<T>(byte[] content)
        {
            var deserContext = new DeserializationContext()
            {
                Config = _serConfig
            };

            return SerializationUtility.DeserializeValue<T>(content, DataFormat.Binary, deserContext);
        }

        public void SerializeValue<T>(T value, Stream stream)
        {
            var serContext = new SerializationContext()
            {
                Config = _serConfig
            };

            SerializationUtility.SerializeValue(value, stream, DataFormat.Binary, serContext);
        }
    }
}
