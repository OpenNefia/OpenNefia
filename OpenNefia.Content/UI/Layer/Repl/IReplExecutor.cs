using CSharpRepl.Services.Completion;

namespace OpenNefia.Content.UI.Layer.Repl
{
    public interface IReplExecutor
    {
        void Initialize();
        ReplExecutionResult Execute(string code);
        IReadOnlyCollection<CompletionItemWithDescription> Complete(string text, int caret);
    }
}
