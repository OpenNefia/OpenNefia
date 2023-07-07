using OpenNefia.Core.IoC;
using OpenNefia.Core.IoC.Exceptions;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Utility;
using CommandLine;
using OpenNefia.Core.GameObjects;
using CommandLine.Text;
using CSharpRepl.Services;

namespace OpenNefia.Core.Console
{
    /// <inheritdoc />
    public sealed class ConsoleHost : IConsoleHost
    {
        public const string SawmillName = "con";

        [Dependency] private readonly IConsoleEx _output = default!;
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;

        private readonly Dictionary<Type, IConsoleCommand> _availableCommands = new();

        /// <inheritdoc />
        public IConsoleShell LocalShell { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<Type, IConsoleCommand> RegisteredCommands => _availableCommands;

        public event ConAnyCommandCallback? AnyCommandExecuted;

        private Type[] CommandArgumentsTypes => _availableCommands.Keys.ToArray();

        public ConsoleHost()
        {
            LocalShell = new ConsoleShell(this);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            LoadConsoleCommands();
        }

        /// <inheritdoc />
        public event EventHandler? ClearText;

        // Get the TArgs in IConsoleCommand<TArgs>.
        private Type? GetArgsTypeForCommand(Type commandDerivedType)
        {
            return commandDerivedType.GetInterfaces()
                .Where(ty => ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(IConsoleCommand<>))
                .Select(ty => ty.GetGenericArguments().FirstOrDefault())
                .FirstOrDefault();
        }

        /// <inheritdoc />
        public void LoadConsoleCommands()
        {
            // search for all client commands in all assemblies, and register them
            foreach (var type in _reflectionManager.GetAllChildren<IConsoleCommand>())
            {
                var argsTy = GetArgsTypeForCommand(type);
                if (argsTy == null)
                    continue;

                var instance = (IConsoleCommand)Activator.CreateInstance(type)!;
                EntitySystem.InjectDependencies(instance);
                if (RegisteredCommands.TryGetValue(type, out var duplicate))
                {
                    throw new InvalidImplementationException(type, typeof(IConsoleCommand),
                        $"Command name already registered: {type}, previous: {duplicate.GetType()}");
                }

                _availableCommands[argsTy] = instance;
            }
        }

        /// <inheritdoc />
        public void ExecuteCommand(string commandStr)
        {
            var shell = LocalShell;

            var types = CommandArgumentsTypes;
            if (types.Length == 0)
            {
                shell.WriteError("No command types registered!");
                return;
            }

            try
            {
                var args = new List<string>();
                CommandParsing.ParseArguments(commandStr, args);

                var parser = new Parser(with => with.HelpWriter = null);
                var result = parser.ParseArguments(args, types);

                result.WithNotParsed(errs =>
                {
                    var builder = SentenceBuilder.Create();
                    var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);

                    shell.WriteError($"Error parsing command: {string.Join('\n', errorMessages)}");
                });

                result.WithParsed(cmdArgs =>
                {
                    var argsType = cmdArgs.GetType();
                    var cmd = _availableCommands[argsType];
                    var cmdType = cmd.GetType();

                    var executeMethod = cmd.GetType()
                        .GetMethod("Execute", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)!;

                    AnyCommandExecuted?.Invoke(shell, cmdType, commandStr, cmdArgs);
                    executeMethod.Invoke(cmd, new[] { shell, cmdArgs });
                });
            }
            catch (Exception e)
            {
                Logger.ErrorS(SawmillName, $"{nameof(ConsoleHost)} ExecuteError - {commandStr}:\n{e}");
                shell.WriteError($"There was an error while executing the command: {e}");
            }
        }

        public void WriteLine(string text) => _output.WriteLine(text);
        public void WriteError(string text) => _output.WriteError(text);

        /// <inheritdoc />
        public void ClearLocalConsole()
        {
            ClearText?.Invoke(this, EventArgs.Empty);
        }
    }
}
