﻿using CSharpRepl.Services;
using CSharpRepl.Services.Completion;
using CSharpRepl.Services.Logging;
using CSharpRepl.Services.Roslyn;
using CSharpRepl.Services.Roslyn.Scripting;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Util;
using PrettyPrompt.Consoles;

namespace OpenNefia.Core.DebugServer
{
    public class CSharpReplExecutor : IReplExecutor
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IConsole _console = default!;
        [Dependency] private readonly ILogManager _logManager = default!;

        private Configuration _config = default!;
        private RoslynServices _roslyn = default!;
        private bool IsInitialized = false;

        internal sealed class OpenNefiaLogger : ITraceLogger
        {
            private readonly ISawmill _sawmill;

            public OpenNefiaLogger(ISawmill sawmill)
            {
                _sawmill = sawmill;
            }

            public void Log(string message)
            {
                _sawmill.Log(LogLevel.Debug, message);
            }

            public void Log(Func<string> message)
            {
                _sawmill.Log(LogLevel.Debug, message());
            }

            public void LogPaths(string message, Func<IEnumerable<string?>> paths)
            {
                _sawmill.Log(LogLevel.Debug, $"{message}, { string.Join(", ", paths())}");
            }
        }

        private static Configuration BuildDefaultConfig(IReflectionManager reflectionManager)
        {
            var references = new HashSet<string>();

            foreach (var assembly in reflectionManager.Assemblies)
            {
                references.Add(assembly.FullName!);
            }

            return new Configuration()
            {
                References = references,
                Usings = new HashSet<string>()
                {
                    "OpenNefia.Core",
                    "OpenNefia.Core.GameObjects",
                    "OpenNefia.Core.Prototypes",
                    "OpenNefia.Core.Utility",
                    "OpenNefia.Core.UI",
                    "OpenNefia.Core.IoC",
                    "OpenNefia.Core.Game",
                    "OpenNefia.Core.Maps",
                    "OpenNefia.Content.GameObjects",
                    "OpenNefia.Content.UI",
                    "OpenNefia.Content.Logic",
                    "System.Linq",
                }
            };
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            var logger = new OpenNefiaLogger(_logManager.GetSawmill("exec.repl"));
            _config = BuildDefaultConfig(_reflectionManager);
            _roslyn = new RoslynServices(_console, _config, logger);

            TaskRunnerLayer.Run(_roslyn.WarmUpAsync(_config.LoadScriptArgs));
            var loadReferenceScript = string.Join("\r\n", _config.References.Select(reference => $@"#r ""{reference}"""));
            var loadReferenceScriptResult = TaskRunnerLayer.Run(_roslyn.EvaluateAsync(loadReferenceScript));

            IsInitialized = true;
        }

        public IReadOnlyCollection<CompletionItemWithDescription> Complete(string text, int caret)
        {
            return _roslyn.CompleteAsync(text, caret).GetAwaiter().GetResult();
        }

        private async Task PrintAsync(EvaluationResult result, bool displayDetails)
        {
            switch (result)
            {
                case EvaluationResult.Success ok:
                    var formatted = await _roslyn.PrettyPrintAsync(ok?.ReturnValue, displayDetails);
                    _console.WriteLine(FormatResultObject(formatted));
                    break;
                case EvaluationResult.Error err:
                    var formattedError = await _roslyn.PrettyPrintAsync(err.Exception, displayDetails);
                    _console.WriteErrorLine(AnsiEscapeCodes.Red + formattedError + AnsiEscapeCodes.Reset);
                    break;
                case EvaluationResult.Cancelled:
                    _console.WriteErrorLine(
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
            var result = _roslyn.EvaluateAsync(code, _config.LoadScriptArgs, new CancellationToken()).GetAwaiter().GetResult();

            PrintAsync(result, true).GetAwaiter().GetResult();

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
