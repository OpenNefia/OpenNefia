using PrettyPrompt.Consoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer.Repl
{
    internal class ReplLayerConsoleBridge : IConsole
    {
        private ReplLayer ReplLayer;

        public ReplLayerConsoleBridge(ReplLayer repl)
        {
            ReplLayer = repl;
        }

        public int CursorTop => ReplLayer.ScrollbackSize;

        public int BufferWidth => ReplLayer.Width / ReplLayer.FontReplText.LoveFont.GetWidth(" ");

        public int WindowHeight => ReplLayer.MaxLines;

        public int WindowTop => ReplLayer.ScrollbackSize - ReplLayer.MaxLines;

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
            ReplLayer.Clear();
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

        public void Write(string value) => ReplLayer.PrintText(value);
        public void WriteLine(string value) => ReplLayer.PrintText(value);
        public void WriteError(string value) => ReplLayer.PrintError(value);
        public void WriteErrorLine(string value) => ReplLayer.PrintError(value);
    }
}
