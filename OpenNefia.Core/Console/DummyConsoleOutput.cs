using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace OpenNefia.Core.Console
{
    public sealed class DummyConsoleOutput : IConsoleOutput
    {
        public void WriteLine(string text)
        {
            Logger.InfoS("con.exec", text);
        }

        public void WriteError(string text)
        {
            Logger.ErrorS("con.exec", text);
        }
    }
}
