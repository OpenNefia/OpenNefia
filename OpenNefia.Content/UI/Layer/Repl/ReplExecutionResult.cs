using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer.Repl
{
    public abstract record ReplExecutionResult
    {
        public sealed record Success(string Result) : ReplExecutionResult;
        public sealed record Error(Exception Exception) : ReplExecutionResult;
    }
}
