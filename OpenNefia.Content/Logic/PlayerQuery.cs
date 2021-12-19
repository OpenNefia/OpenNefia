using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.Logic
{
    public class YesOrNoOptions
    {
        public string QueryText { get; set; } = string.Empty;

        public bool IsCancellable { get; set; } = true;

        public bool Invert { get; set; } = false;
    }

    public interface IPlayerQuery
    {
        bool YesOrNo(string queryText);
        bool YesOrNo(YesOrNoOptions opts);
    }

    public class PlayerQuery : IPlayerQuery
    {
        private enum YesNo
        {
            Yes,
            No
        }

        public bool YesOrNo(string queryText)
        {
            return YesOrNo(new YesOrNoOptions() { QueryText = queryText });
        }

        public bool YesOrNo(YesOrNoOptions opts)
        {
            var items = new List<PromptChoice<YesNo>>()
            {
                new PromptChoice<YesNo>(YesNo.Yes, text: "Yes", key: Keys.Y),
                new PromptChoice<YesNo>(YesNo.No, text: "No..", key: Keys.N),
            };
            if (opts.Invert)
            {
                items.Reverse();
            }

            var result = new Prompt<YesNo>(items, new PromptOptions() { QueryText = opts.QueryText, IsCancellable = opts.IsCancellable }).Query();

            if (result.HasValue)
            {
                return result.Value.ChoiceData == YesNo.Yes;
            }

            return false;
        }
    }
}
