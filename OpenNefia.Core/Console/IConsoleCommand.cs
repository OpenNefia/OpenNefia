using JetBrains.Annotations;

namespace OpenNefia.Core.Console
{
    /// <summary>
    /// Basic interface to handle console commands. Any class implementing this will be
    /// registered with the console system through reflection.
    /// </summary>
    public interface IConsoleCommand
    {
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public interface IConsoleCommand<T> : IConsoleCommand
        where T: notnull
    {
        /// <summary>
        /// Executes the client command.
        /// </summary>
        /// <param name="shell">The console that executed this command.</param>
        /// <param name="args">The parsed arguments.</param>
        public void Execute(IConsoleShell shell, T args);
    }
}
