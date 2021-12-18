using OpenNefia.Core.IoC;
using PrettyPrompt.Consoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer.Repl
{
    internal class ReplLayerConsole : IConsole
    {
        [Dependency] private readonly IReplLayer _replLayer = default!;

        public int CursorTop => _replLayer.ScrollbackSize;

        public int BufferWidth => _replLayer.Width / _replLayer.FontReplText.LoveFont.GetWidth(" ");

        public int WindowHeight => _replLayer.MaxLines;

        public int WindowTop => _replLayer.ScrollbackSize - _replLayer.MaxLines;

        public bool KeyAvailable => true;

        public bool CaptureControlC { get => false; set { } }

        public event ConsoleCancelEventHandler CancelKeyPress
        {
            add
            {
                Console.CancelKeyPress += value;
            }
            remove
            {
                Console.CancelKeyPress -= value;
            }
        }

        public void Clear()
        {
            _replLayer.Clear();
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

        public void Write(string value) => _replLayer.PrintText(value);
        public void WriteLine(string value) => _replLayer.PrintText(value);
        public void WriteError(string value) => _replLayer.PrintText(value, UiColors.ReplTextError);
        public void WriteErrorLine(string value) => _replLayer.PrintText(value, UiColors.ReplTextError);
    }
}
