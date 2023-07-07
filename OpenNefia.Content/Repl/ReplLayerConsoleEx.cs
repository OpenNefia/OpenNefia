using CSharpRepl.Services;
using OpenNefia.Content.UI;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using PrettyPrompt.Consoles;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text;

namespace OpenNefia.Content.Repl
{
    public sealed class ReplLayerConsoleEx : IConsoleEx
    {
        [Dependency] protected IReplLayer _replLayer = default!;

        public IConsole PrettyPromptConsole { get; }
        public Profile Profile { get; }
        public IAnsiConsoleCursor Cursor { get; } 
        public IAnsiConsoleInput Input { get; } 
        public IExclusivityMode ExclusivityMode { get; }
        public RenderPipeline Pipeline { get; }

        public ReplLayerConsoleEx()
        {
            PrettyPromptConsole = new ReplLayerConsole(this);
            Profile = new Profile(new ReplLayerOutput(this), EncodingHelpers.UTF8);
            Cursor = new ReplLayerCursor(this);
            Input = new ReplLayerInput(this);
            ExclusivityMode = new ReplLayerExclusivityMode(this);
            Pipeline = new RenderPipeline();
        }

        public void Clear(bool home)
        {
            _replLayer.Clear();
        }

        public void Write(IRenderable renderable)
        {
            var result = Build(renderable);
            Profile.Out.Writer.Write(result);
        }

        private string Build(IRenderable renderable)
        {
            var sb = new StringBuilder();
            foreach (var segment in renderable.GetSegments(this))
            {
                var parts = segment.Text.Split('\n');
                foreach (var part in parts)
                {
                    sb.AppendLine(part);
                }
            }
            return sb.ToString();
        }

        private class ReplLayerConsole : IConsole
        {
            private ReplLayerConsoleEx replLayerConsoleEx;

            public ReplLayerConsole(ReplLayerConsoleEx replLayerConsoleEx)
            {
                this.replLayerConsoleEx = replLayerConsoleEx;
            }

            public int CursorTop { get; }
            public int BufferWidth { get; }
            public int WindowHeight { get; }
            public int WindowTop { get; }
            public bool KeyAvailable { get; }
            public bool IsErrorRedirected { get; }
            public bool CaptureControlC { get; set; }

            public event ConsoleCancelEventHandler? CancelKeyPress;

            public void Clear()
            {
                replLayerConsoleEx._replLayer.Clear();
            }

            public void HideCursor()
            {
            }

            public void InitVirtualTerminalProcessing()
            {
            }

            public ConsoleKeyInfo ReadKey(bool intercept)
            {
                throw new NotImplementedException();
            }

            public void ShowCursor()
            {
            }

            public void Write(string? value)
            {
                replLayerConsoleEx._replLayer.PrintText(value ?? "null", UiColors.ReplTextResult);
            }

            public void WriteError(string? value)
            {
                replLayerConsoleEx._replLayer.PrintText(value ?? "null", UiColors.ReplTextError);
            }

            public void WriteLine(string? value)
            {
                replLayerConsoleEx._replLayer.PrintText(value ?? "null", UiColors.ReplTextResult);
            }

            public void WriteErrorLine(string? value)
            {
                replLayerConsoleEx._replLayer.PrintText(value ?? "null", UiColors.ReplTextError);
            }

            public void Write(ReadOnlySpan<char> value) => Write(value.ToString());
            public void WriteError(ReadOnlySpan<char> value) => WriteError(value.ToString());
            public void WriteLine(ReadOnlySpan<char> value) => WriteLine(value.ToString());
            public void WriteErrorLine(ReadOnlySpan<char> value) => WriteErrorLine(value.ToString());
        }

        private class ReplLayerOutput : IAnsiConsoleOutput
        {
            private ReplLayerConsoleEx replLayerConsoleEx;

            public ReplLayerOutput(ReplLayerConsoleEx replLayerConsoleEx)
            {
                this.replLayerConsoleEx = replLayerConsoleEx;
                this.Writer = new ReplLayerTextWriter();
            }

            public TextWriter Writer { get; }
            public bool IsTerminal => false;
            public int Width => replLayerConsoleEx.Profile.Width;
            public int Height => replLayerConsoleEx.Profile.Height;

            public void SetEncoding(Encoding encoding)
            {
            }
        }

        private class ReplLayerCursor : IAnsiConsoleCursor
        {
            private ReplLayerConsoleEx replLayerConsoleEx;

            public ReplLayerCursor(ReplLayerConsoleEx replLayerConsoleEx)
            {
                this.replLayerConsoleEx = replLayerConsoleEx;
            }

            public void Move(CursorDirection direction, int steps)
            {
                switch (direction)
                {
                    case CursorDirection.Left:
                        replLayerConsoleEx._replLayer.CaretPos--;
                        break;
                    case CursorDirection.Right:
                        replLayerConsoleEx._replLayer.CaretPos++;
                        break;
                    case CursorDirection.Up:
                    case CursorDirection.Down:
                    default:
                        break;
                }
            }

            public void SetPosition(int column, int line)
            {
                replLayerConsoleEx._replLayer.CaretPos = column;
            }

            public void Show(bool show)
            {
            }
        }

        private class ReplLayerTextWriter : TextWriter
        {
            public override Encoding Encoding => EncodingHelpers.UTF8;
        }

        private class ReplLayerInput : IAnsiConsoleInput
        {
            private ReplLayerConsoleEx replLayerConsoleEx;

            public ReplLayerInput(ReplLayerConsoleEx replLayerConsoleEx)
            {
                this.replLayerConsoleEx = replLayerConsoleEx;
            }

            public bool IsKeyAvailable()
            {
                return true;
            }

            public ConsoleKeyInfo? ReadKey(bool intercept)
            {
                return null;
            }

            public Task<ConsoleKeyInfo?> ReadKeyAsync(bool intercept, CancellationToken cancellationToken)
            {
                return Task.FromResult<ConsoleKeyInfo?>(null);
            }
        }

        private class ReplLayerExclusivityMode : IExclusivityMode
        {
            private ReplLayerConsoleEx replLayerConsoleEx;

            public ReplLayerExclusivityMode(ReplLayerConsoleEx replLayerConsoleEx)
            {
                this.replLayerConsoleEx = replLayerConsoleEx;
            }

            public T Run<T>(Func<T> func)
            {
                return func();
            }

            public async Task<T> RunAsync<T>(Func<Task<T>> func)
            {
                return await func();
            }
        }
    }
}
