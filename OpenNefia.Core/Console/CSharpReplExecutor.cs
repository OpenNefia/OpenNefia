using CSharpRepl.Services.Completion;
using CSharpRepl.Services.Logging;
using CSharpRepl.Services.Roslyn;
using CSharpRepl.Services.Roslyn.Scripting;
using Microsoft.CodeAnalysis.CSharp;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using PrettyPrompt.Consoles;
using PrettyPrompt.Highlighting;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using CSharpReplConfig = CSharpRepl.Services.Configuration;

namespace OpenNefia.Core.Console
{
    internal interface ICSharpReplExecutor : IReplExecutor
    {
        RoslynServices RoslynServices { get; }
    }

    internal class CSharpReplExecutor : ICSharpReplExecutor
    {
        public const string SawmillName = "exec.repl";

        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IConsole _console = default!;
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly ITaskRunner _taskRunner = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IResourceManager _resources = default!;

        private CSharpReplConfig _replConfig = default!;
        private RoslynServices _roslynServices = default!;
        private bool _showDetails = true;
        private bool IsInitialized = false;

        public RoslynServices RoslynServices => _roslynServices;

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

        private static CSharpReplConfig BuildDefaultConfig(IReflectionManager reflectionManager)
        {
            var references = new HashSet<string>();

            // Modules might be located under Resources/Assemblies, so be
            // sure to use an executable directory-relative path to reference
            // them.
            var exeDir = ResourcePath.FromRelativeSystemPath(AppDomain.CurrentDomain.BaseDirectory);

            foreach (var assembly in reflectionManager.Assemblies)
            {
                var exeRelativePath = ResourcePath.FromRelativeSystemPath(assembly.Location)
                    .RelativeTo(exeDir)
                    .ToRelativeSystemPath();

                references.Add(exeRelativePath);
            }

            return new CSharpReplConfig(
                references: references.ToArray(),
                usings: new string[]
                {
                    "OpenNefia.Core",
                    "OpenNefia.Core.GameObjects",
                    "OpenNefia.Core.Prototypes",
                    "OpenNefia.Core.Utility",
                    "OpenNefia.Core.UI",
                    "OpenNefia.Core.IoC",
                    "OpenNefia.Core.Game",
                    "OpenNefia.Core.Maps",
                    "OpenNefia.Core.Locale",
                    "OpenNefia.Content.GameObjects",
                    "OpenNefia.Content.UI",
                    "OpenNefia.Content.Logic",
                    "System.Linq"
                }
            );
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            Logger.DebugS(SawmillName, "Initializing REPL executor...");

            using (var profiler = new ProfilerLogger(LogLevel.Debug, SawmillName, "REPL executor initialization"))
            {
                var logger = new OpenNefiaLogger(_logManager.GetSawmill(SawmillName));
                _replConfig = BuildDefaultConfig(_reflectionManager);
                _roslynServices = new RoslynServices(_console, _replConfig, logger);

                _taskRunner.Run(_roslynServices.WarmUpAsync(_replConfig.LoadScriptArgs));
                var loadReferenceScript = string.Join("\r\n", _replConfig.References.Select(reference => $@"#r ""{reference}"""));
                var loadReferenceScriptResult = _taskRunner.Run(_roslynServices.EvaluateAsync(loadReferenceScript));

                var autoloadScriptPathString = _config.GetCVar(CVars.ReplAutoloadScript);

                if (!string.IsNullOrEmpty(autoloadScriptPathString))
                {
                    var autoloadScriptPath = new ResourcePath(autoloadScriptPathString);
                    if (_resources.ContentFileExists(autoloadScriptPath))
                    {
                        _console.WriteLine($"Loading startup script: {autoloadScriptPath}");
                        var autoloadScript = _resources.ContentFileReadAllText(autoloadScriptPath);
                        var autoloadScriptResult = _taskRunner.Run(_roslynServices.EvaluateAsync(autoloadScript));
                        _taskRunner.Run(PrintAsync(autoloadScriptResult, false));
                    }
                    else
                    {
                        _console.WriteErrorLine($"Startup script not found: {autoloadScriptPathString}");
                    }
                }

                IsInitialized = true;
            }
        }

        public IReadOnlyCollection<CompletionItemWithDescription> Complete(string text, int caret)
        {
            return _roslynServices.CompleteAsync(text, caret).GetAwaiter().GetResult();
        }

        private async Task PrintAsync(EvaluationResult result, bool displayDetails)
        {
            switch (result)
            {
                case EvaluationResult.Success ok:
                    var formatted = await _roslynServices.PrettyPrintAsync(ok?.ReturnValue, displayDetails);
                    _console.WriteLine(FormatResultObject(formatted));
                    break;
                case EvaluationResult.Error err:
                    var formattedError = await _roslynServices.PrettyPrintAsync(err.Exception, displayDetails);
                    _console.WriteErrorLine(AnsiColor.Red.GetEscapeSequence() + formattedError + AnsiEscapeCodes.Reset);
                    break;
                case EvaluationResult.Cancelled:
                    _console.WriteErrorLine(
                       AnsiColor.Yellow.GetEscapeSequence() + "Operation cancelled." + AnsiEscapeCodes.Reset
                    );
                    break;
            }
        }

        private string FormatResultObject(object? returnValue)
        {
            if (returnValue == null)
                return "null";
            else if (returnValue is string)
                return $"\"{returnValue}\"";

            // Use ToString() if it's overridden on the type, else do property dumping.
            var strValue = returnValue.ToString() ?? "<???>";
            var toString = returnValue.GetType().GetMethod("ToString", Type.EmptyTypes);
            if (toString == null || toString.DeclaringType != returnValue.GetType())
                return _roslynServices.PrettyPrintAsync(returnValue, _showDetails).GetAwaiter().GetResult() ?? strValue;

            return strValue;
        }

        public ReplExecutionResult Execute(string code)
        {
            var result = _roslynServices.EvaluateAsync(code, _replConfig.LoadScriptArgs, new CancellationToken()).GetAwaiter().GetResult();

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
