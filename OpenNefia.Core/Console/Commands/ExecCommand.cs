using CommandLine;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Console;

namespace OpenNefia.Core.Console.Commands
{
    internal class ExecCommand : IConsoleCommand<ExecCommand.Args>
    {
        [Dependency] private readonly ICSharpReplExecutor _replExecutor = default!;

        [Verb(name: "exec", HelpText = "Runs a C# script in the engine's environment.")]
        public class Args
        {
            [Value(0, MetaName = "scriptFilePath", Required = true, HelpText = "Path to a C# script file in the Roslyn format.")]
            public string ScriptFilePath { get; set; } = default!;
        }

        public void Execute(IConsoleShell shell, Args args)
        {
            _replExecutor.Initialize();
            var script = File.ReadAllText(args.ScriptFilePath);
            var result = _replExecutor.Execute(script);

            switch (result)
            {
                case ReplExecutionResult.Success success:
                    shell.WriteLine($"Exec result: {success.Result}");
                    break;
                case ReplExecutionResult.Error err:
                    shell.WriteError($"Exec error: {err.Exception.Message}");
                    break;
                default:
                    shell.WriteError("Exec failed");
                    break;
            }
        }
    }
}
