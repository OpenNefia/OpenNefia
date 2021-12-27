using CommandLine;
using OpenNefia.Core.DebugServer;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.CommandLine.Commands
{
    [Verb(name: "exec", HelpText = "Runs a C# script in the engine's environment.")]
    internal class ExecCommand : BaseCommand
    {
        [Dependency] private readonly IReplExecutor _replExecutor = default!;

        [Value(0, MetaName = "scriptFilePath", Required = true, HelpText = "Path to a C# script file in the Roslyn format.")]
        public string ScriptFilePath { get; set; } = default!;

        public override void Execute()
        {
            _replExecutor.Initialize();
            var script = File.ReadAllText(ScriptFilePath);
            var result = _replExecutor.Execute(script);

            switch (result)
            {
                case ReplExecutionResult.Success success:
                    Console.WriteLine($"Exec result: {success.Result}");
                    break;
                case ReplExecutionResult.Error err:
                    Console.WriteLine($"Exec error: {err.Exception.Message}");
                    throw err.Exception;
                default:
                    throw new InvalidOperationException("Exec failed");
            }
        }
    }
}
