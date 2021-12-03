using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Core.Logic
{
    public static class PlayerQuery
    {
        private enum YesNo
        {
            Yes,
            No
        }

        public static bool YesOrNo(string queryText, bool canCancel = true, bool invert = false)
        {
            var items = new List<PromptChoice<YesNo>>()
            {
                new PromptChoice<YesNo>(YesNo.Yes, text: "Yes", key: Keys.Y),
                new PromptChoice<YesNo>(YesNo.No, text: "No..", key: Keys.N),
            };
            if (invert)
            {
                items.Reverse();
            }

            var result = new Prompt<YesNo>(items, new PromptOptions() { QueryText = queryText, IsCancellable = canCancel }).Query();

            if (result.HasValue)
            {
                return result.Value.ChoiceData == YesNo.Yes;
            }

            return false;
        }
    }
}
