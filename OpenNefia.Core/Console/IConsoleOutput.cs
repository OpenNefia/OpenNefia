namespace OpenNefia.Core.Console
{
    public interface IConsoleOutput
    {
        void WriteLine(string text);
        void WriteError(string text);
    }
}