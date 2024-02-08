using OpenNefia.Core.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.UserInterface
{
    [KeyFunctions]
    public static class VisualAIKeyFunctions
    {
        public static readonly BoundKeyFunction Insert = $"VisualAI.{nameof(Insert)}";
        public static readonly BoundKeyFunction InsertDown = $"VisualAI.{nameof(InsertDown)}";
        public static readonly BoundKeyFunction Delete = $"VisualAI.{nameof(Delete)}";
        public static readonly BoundKeyFunction DeleteAndMergeDown = $"VisualAI.{nameof(DeleteAndMergeDown)}";
        public static readonly BoundKeyFunction DeleteToRight = $"VisualAI.{nameof(DeleteToRight)}";
        public static readonly BoundKeyFunction SwapBranches = $"VisualAI.{nameof(SwapBranches)}";

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
