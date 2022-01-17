namespace OpenNefia.Core.Console
{
    /// <inheritdoc />
    public class ConsoleShell : IConsoleShell
    {
        /// <summary>
        /// Constructs a new instance of <see cref="ConsoleShell"/>.
        /// </summary>
        /// <param name="host">Console Host that owns this shell.</param>
        public ConsoleShell(IConsoleHost host)
        {
            ConsoleHost = host;
        }

        /// <inheritdoc />
        public IConsoleHost ConsoleHost { get; }

        /// <inheritdoc />
        public void ExecuteCommand(string command)
        {
            ConsoleHost.ExecuteCommand(command);
        }

        /// <inheritdoc />
        public void WriteLine(string text)
        {
            ConsoleHost.WriteLine(text);
        }

        /// <inheritdoc />
        public void WriteError(string text)
        {
            ConsoleHost.WriteError(text);
        }

        /// <inheritdoc />
        public void Clear()
        {
            ConsoleHost.ClearLocalConsole();
        }
    }
}
