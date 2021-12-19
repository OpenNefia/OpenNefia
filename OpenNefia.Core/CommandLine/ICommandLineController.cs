using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.CommandLine
{
    public interface ICommandLineController
    {
        IEnumerable<Type> Verbs { get; }

        bool TryParseCommand(string[] args, [NotNullWhen(true)] out ICommand? command);
        public void Run(string[] args);
    }
}
