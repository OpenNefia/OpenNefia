using CSharpRepl.Services.Completion;

namespace OpenNefia.Core.Console
{
    public interface IReplExecutor
    {
        void Initialize();

        ReplExecutionResult Execute(string code);
        ReplExecutionResult LoadStartupScript();

        IReadOnlyCollection<CompletionItemWithDescription> Complete(string text, int caret);
    }
}
