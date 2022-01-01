using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Core.Input.Keyboard;

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
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

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
                new PromptChoice<YesNo>(YesNo.Yes, text: "Yes", key: Key.Y),
                new PromptChoice<YesNo>(YesNo.No, text: "No..", key: Key.N),
            };
            if (opts.Invert)
            {
                items.Reverse();
            }

            var promptOpts = new Prompt<YesNo>.Args(items) 
            { 
                QueryText = opts.QueryText, 
                IsCancellable = opts.IsCancellable 
            };
            var result = _uiManager.Query<Prompt<YesNo>, Prompt<YesNo>.Args, PromptChoice<YesNo>>(promptOpts);

            if (result.HasValue)
            {
                return result.Value.ChoiceData == YesNo.Yes;
            }

            return false;
        }
    }
}
