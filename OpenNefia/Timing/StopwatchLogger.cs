using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Timing
{
    public sealed class StopwatchLogger : IDisposable
    {
        private readonly ISawmill? _sawmill;
        private readonly string _message;
        private readonly LogLevel _level;
        private readonly IStopwatch _stopwatch;

        public StopwatchLogger(LogLevel level, string sawmill, string message)
        {
            _message = message;
            _level = level;
            _stopwatch = new Stopwatch();
            _sawmill = Logger.GetSawmill(sawmill);

            _stopwatch.Start();
        }

        public StopwatchLogger(LogLevel level, string message)
        {
            _message = message;
            _level = level;
            _stopwatch = new Stopwatch();
            _sawmill = null;

            _stopwatch.Start();
        }

        public void Dispose()
        {
            var mes = $"'{_message}' finished in {_stopwatch.Elapsed.ToString("s\\.ff")}s.";

            if (_sawmill != null)
            {
                _sawmill.Log(_level, mes);
            }
            else
            {
                Logger.Log(_level, mes);
            }
        }
    }
}
