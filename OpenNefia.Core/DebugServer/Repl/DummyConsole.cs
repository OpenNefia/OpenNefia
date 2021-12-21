using OpenNefia.Core.Log;
using PrettyPrompt.Consoles;

namespace OpenNefia.Core.DebugServer
{
    internal class DummyConsole : IConsole
    {
        public int CursorTop => 0;
        public int BufferWidth => 0;
        public int WindowHeight => 0;
        public int WindowTop => 0;
        public bool KeyAvailable => false;
        public bool CaptureControlC { get => false; set { } }

        public event ConsoleCancelEventHandler? CancelKeyPress;

        public void Clear()
        {
        }

        public void HideCursor()
        {
        }

        public void InitVirtualTerminalProcessing()
        {
        }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            return new ConsoleKeyInfo();
        }

        public void ShowCursor()
        {
        }

        public void Write(string value)
        {
            Logger.InfoS("repl.exec", value);
        }

        public void WriteLine(string value)
        {
            Logger.InfoS("repl.exec", value);
        }

        public void WriteError(string value)
        {
            Logger.ErrorS("repl.exec", value);
        }

        public void WriteErrorLine(string value)
        {
            Logger.ErrorS("repl.exec", value);
        }
    }
}
