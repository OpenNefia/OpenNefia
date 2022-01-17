using System;
using System.Collections.Generic;

namespace OpenNefia.Core.Console
{
    /// <summary>
    /// A delegate that is called when the command is executed inside a shell.
    /// </summary>
    /// <param name="shell">The console shell that executed this command.</param>
    /// <param name="argStr">Unparsed text of the complete command with arguments.</param>
    /// <param name="args">An array of all the parsed arguments.</param>
    public delegate void ConCommandCallback(IConsoleShell shell, object args);

    public delegate void ConAnyCommandCallback(IConsoleShell shell, Type commandType, string argStr, object args);

    /// <summary>
    /// The console host exists as a singleton subsystem that provides all of the features of the console API.
    /// It will register console commands, spawn console shells and execute command strings.
    /// </summary>
    public interface IConsoleHost
    {
        /// <summary>
        /// The local shell of the peer that is always available.
        /// </summary>
        IConsoleShell LocalShell { get; }

        /// <summary>
        /// A map of (commandArgsType -> ICommand) of every registered command in the shell.
        /// </summary>
        IReadOnlyDictionary<Type, IConsoleCommand> RegisteredCommands { get; }

        /// <summary>
        /// Invoked before any console command is executed.
        /// </summary>
        event ConAnyCommandCallback AnyCommandExecuted;
        event EventHandler ClearText;

        void Initialize();

        /// <summary>
        /// Scans all loaded assemblies for console commands and registers them. This will NOT sync with connected clients, and
        /// should only be used during server initialization.
        /// </summary>
        void LoadConsoleCommands();

        /// <summary>
        /// Execute a command string on the local shell.
        /// </summary>
        /// <param name="command">Command string to execute.</param>
        void ExecuteCommand(string command);

        /// <summary>
        /// Sends a text string to the remote session.
        /// </summary>
        /// <param name="session">
        /// Remote session to send the text message to. If this is null, the text is sent to the local
        /// console.
        /// </param>
        /// <param name="text">Text message to send.</param>
        void WriteLine(string text);

        /// <summary>
        /// Sends a foreground colored text string to the remote session.
        /// </summary>
        /// <param name="session">
        /// Remote session to send the text message to. If this is null, the text is sent to the local
        /// console.
        /// </param>
        /// <param name="text">Text message to send.</param>
        void WriteError(string text);

        /// <summary>
        /// Removes all text from the local console.
        /// </summary>
        void ClearLocalConsole();
    }
}
