using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.CommandLine
{
    public interface ICommand
    {
        bool CanRunInBatchMode => true;
        LogLevel LogLevel { get; }

        void Execute();
    }
}
