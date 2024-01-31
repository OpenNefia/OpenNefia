using OpenNefia.Core.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.UserInterface
{
    public static class VisualAIKeyFunctions
    {
        public static readonly BoundKeyFunction Insert = nameof(Insert);
        public static readonly BoundKeyFunction InsertDown = nameof(InsertDown);
        public static readonly BoundKeyFunction Delete = nameof(Delete);
        public static readonly BoundKeyFunction DeleteAndMergeDown = nameof(DeleteAndMergeDown);
        public static readonly BoundKeyFunction DeleteToRight = nameof(DeleteToRight);
        public static readonly BoundKeyFunction SwapBranches = nameof(SwapBranches);

        internal static void SetupContexts(IInputContextContainer contexts)
        {
            var common = contexts.GetContext("common");
            common.AddFunction(Insert);
            common.AddFunction(InsertDown);
            common.AddFunction(Delete);
            common.AddFunction(DeleteAndMergeDown);
            common.AddFunction(DeleteToRight);
            common.AddFunction(SwapBranches);
        }
    }
}
