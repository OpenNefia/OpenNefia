namespace OpenNefia.Core.Console
{
    /// <summary>
    /// The console shell that executes commands.
    /// </summary>
    public interface IConsoleShell
    {
        /// <summary>
        /// The console host that owns this shell.
        /// </summary>
        IConsoleHost ConsoleHost { get; }

        /// <summary>
        /// Executes a command string on this specific session shell. If the command does not exist, the command will be forwarded
        /// to the
        /// remote shell.
        /// </summary>
        /// <param name="command">command line string to execute.</param>
        void ExecuteCommand(string command);

        /// <summary>
        /// Writes a line to the output of the console.
        /// </summary>
        /// <param name="text">Line of text to write.</param>
        void WriteLine(string text);

        /// <summary>
        /// Write an error line to the console window.
        /// </summary>
        /// <param name="text">Line of text to write.</param>
        void WriteError(string text);

        /// <summary>
        /// Clears the entire console of text.
        /// </summary>
        void Clear();
    }
}
