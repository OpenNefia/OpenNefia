using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Serialization.Instanced
{
    internal class InstancedSerializerLogger : OdinSerializer.ILogger
    {
        private ISawmill _sawmill;

        public InstancedSerializerLogger()
        {
            _sawmill = Logger.GetSawmill("ser.inst");
        }

        public void LogError(string error)
        {
            _sawmill.Error(error);
        }

        public void LogException(Exception exception)
        {
            _sawmill.Log(LogLevel.Error, exception, "Exception occurred.");
        }

        public void LogWarning(string warning)
        {
            _sawmill.Warning(warning);
        }
    }
}
