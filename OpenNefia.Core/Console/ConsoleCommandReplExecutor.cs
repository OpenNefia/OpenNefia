using CSharpRepl.Services.Completion;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Console
{
    public class ConsoleCommandReplExecutor : IReplExecutor
    {
        [Dependency] private readonly IConsoleHost _conHost = default!;

        public void Initialize()
        {
        }

        public IReadOnlyCollection<CompletionItemWithDescription> Complete(string text, int caret)
        {
            return new List<CompletionItemWithDescription>();
        }

        public ReplExecutionResult Execute(string code)
        {
            _conHost.ExecuteCommand(code);
            return new ReplExecutionResult.Success("");
        }
    }
}
