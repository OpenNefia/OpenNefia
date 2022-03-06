using OpenNefia.Core.IoC;
using PrettyPrompt.Consoles;

namespace OpenNefia.Core.Console
{
    public sealed class DummyConsole : IConsole
    {
        [Dependency] private readonly IConsoleOutput _output = default!;

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

        public void Write(string? value) => _output.WriteLine(value ?? string.Empty);
        public void WriteLine(string? value) => _output.WriteLine(value ?? string.Empty);
        public void WriteError(string? value) => _output.WriteError(value ?? string.Empty);
        public void WriteErrorLine(string? value) => _output.WriteError(value ?? string.Empty);
    }
}
