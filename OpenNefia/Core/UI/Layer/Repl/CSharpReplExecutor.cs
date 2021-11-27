using CSharpRepl.Services;
using CSharpRepl.Services.Completion;
using CSharpRepl.Services.Logging;
using CSharpRepl.Services.Roslyn;
using CSharpRepl.Services.Roslyn.Scripting;
using FluentResults;
using OpenNefia.Core.Util;
using PrettyPrompt.Consoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer.Repl
{
    public class CSharpReplExecutor : IReplExecutor
    {
        private IConsole Console;
        private Configuration Config;
        private RoslynServices Roslyn;
        private bool IsInitialized = false;

        internal sealed class NullLogger : ITraceLogger
        {
            public void Log(string message) {}
            public void Log(Func<string> message) {}
            public void LogPaths(string message, Func<IEnumerable<string?>> paths) {}
        }

        public CSharpReplExecutor(ReplLayer replLayer) : this(replLayer, BuildDefaultConfig())
        {
        }

        private static Configuration BuildDefaultConfig()
        {
            var references = new HashSet<string>();

            var openNefia = Assembly.GetEntryAssembly()!;
            references.Add(openNefia.GetName().Name!);

            foreach (var reference in openNefia.GetReferencedAssemblies())
            {
                references.Add(reference.Name!);
            }

            return new Configuration()
            {
                References = references,
                Usings = new HashSet<string>() 
                {
                    "OpenNefia.Core",
                    "OpenNefia.Core.Data",
                    "OpenNefia.Core.Data.Types",
                    "OpenNefia.Core.Extensions",
                    "OpenNefia.Core.Object",
                    "OpenNefia.Core.UI",
                    "System.Linq",
                }
            };
        }

        public CSharpReplExecutor(ReplLayer replLayer, Configuration config)
        {
            Console = new ReplLayerConsoleBridge(replLayer);
            Config = config;
            Roslyn = new RoslynServices(Console, Config, new NullLogger());
        }

        public void Init()
        {
            if (this.IsInitialized)
                return;

            TaskRunner.Run(Roslyn.WarmUpAsync(Config.LoadScriptArgs));
            var loadReferenceScript = string.Join("\r\n", Config.References.Select(reference => $@"#r ""{reference}"""));
            var loadReferenceScriptResult = TaskRunner.Run(Roslyn.EvaluateAsync(loadReferenceScript));

            this.IsInitialized = true;
        }

        public IReadOnlyCollection<CompletionItemWithDescription> Complete(string text, int caret)
        {
            return this.Roslyn.CompleteAsync(text, caret).GetAwaiter().GetResult();
        }

        private static async Task PrintAsync(RoslynServices roslyn, IConsole console, EvaluationResult result, bool displayDetails)
        {
            switch (result)
            {
                case EvaluationResult.Success ok:
                    var formatted = await roslyn.PrettyPrintAsync(ok?.ReturnValue, displayDetails);
                    console.WriteLine(FormatResultObject(formatted));
                    break;
                case EvaluationResult.Error err:
                    var formattedError = await roslyn.PrettyPrintAsync(err.Exception, displayDetails);
                    console.WriteErrorLine(AnsiEscapeCodes.Red + formattedError + AnsiEscapeCodes.Reset);
                    break;
                case EvaluationResult.Cancelled:
                    console.WriteErrorLine(
                        AnsiEscapeCodes.Yellow + "Operation cancelled." + AnsiEscapeCodes.Reset
                    );
                    break;
            }
        }

        private static string FormatResultObject(object? returnValue)
        {
            if (returnValue == null)
                return "null";
            return returnValue!.ToString()!;
        }

        public ReplExecutionResult Execute(string code)
        {
            var result = Roslyn.EvaluateAsync(code, Config.LoadScriptArgs, new CancellationToken()).GetAwaiter().GetResult();

            switch (result)
            {
                case EvaluationResult.Success success:
                    
                    return new ReplExecutionResult.Success(FormatResultObject(success.ReturnValue));
                case EvaluationResult.Error err:
                    return new ReplExecutionResult.Error(err.Exception);
                default:
                    return new ReplExecutionResult.Error(new InvalidOperationException("Could not process REPL result"));
            }
        }
    }
}
