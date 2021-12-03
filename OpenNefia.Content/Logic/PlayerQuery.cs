using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.Logic
{
    public interface IPlayerQuery
    {
        bool YesOrNo(string queryText, bool canCancel = true, bool invert = false);
    }

    public class PlayerQuery : IPlayerQuery
    {
        private enum YesNo
        {
            Yes,
            No
        }

        public bool YesOrNo(string queryText, bool canCancel = true, bool invert = false)
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
