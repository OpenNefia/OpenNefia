using OpenNefia.Core.IoC;
using OpenNefia.Core.Console;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Repl
{
    public class ReplLayerConsoleOutput : IConsoleOutput
    {
        [Dependency] private readonly IReplLayer _replLayer = default!;

        public void WriteLine(string text)
        {
            _replLayer.PrintText(text);
        }

        public void WriteError(string text)
        {
            _replLayer.PrintText(text, UiColors.ReplTextError);
        }
    }
}
