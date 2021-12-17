using CommandLine;
using CommandLine.Text;
using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.CommandLine
{
    public abstract class BaseCommand : ICommand
    {
        [Option(longName: "logLevel", shortName: 'l', Default = LogLevel.Info, HelpText = "Log level to run at.")]
        public LogLevel LogLevel { get; set; }

        public virtual bool CanRunInBatchMode => true;
        public abstract void Execute();
    }
}
