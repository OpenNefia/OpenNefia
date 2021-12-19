using CommandLine;
using CommandLine.Text;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Utility;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenNefia.Core.CommandLine
{
    public class CommandLineController : ICommandLineController
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly ILogManager _logManager = default!;

        public IEnumerable<Type> Verbs
        {
            get
            {
                return _reflectionManager.GetAllChildren<ICommand>()
                    .Where(t => t.HasCustomAttribute<VerbAttribute>());
            }
        }

        private string GetHeading()
        {
            var assembly = Assembly.GetEntryAssembly()!;
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return $"{assembly.GetName()} {fvi.FileVersion}";
        }

        private void DisplayUsage<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = GetHeading();
                h.Copyright = "© 2022 OpenNefia Authors. Licensed under the MIT License.";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            });

            Console.WriteLine(helpText);
        }

        public bool TryParseCommand(string[] args, [NotNullWhen(true)] out ICommand? command)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            var verbs = Verbs.ToArray();

            command = parser.ParseArguments(args, verbs)
                .MapResult(obj =>
                {
                    var cmd = (ICommand)obj;
                    cmd = IoCManager.InjectDependencies(cmd!);
                    return (ICommand?)cmd;
                }, obj => null);

            return command != null;
        }

        public void Run(string[] args)
        {
            if (!_gameController.Startup())
            {
                Logger.Fatal("Failed to start game controller!");
                return;
            }

            var parser = new Parser(with => with.HelpWriter = null);
            var verbs = Verbs.ToArray();

            var result = parser.ParseArguments(args, verbs);

            result = result.WithParsed(obj =>
            {
                var cmd = (ICommand)obj;
                cmd = IoCManager.InjectDependencies(cmd);
                _logManager.RootSawmill.Level = cmd.LogLevel;
                cmd.Execute();
            });
            result = result.WithNotParsed(errors => DisplayUsage(result, errors));
        }
    }
}
