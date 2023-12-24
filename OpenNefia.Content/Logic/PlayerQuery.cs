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
        /// <summary>
        /// Prompts the player to choose "Yes" or "No".
        /// If the prompt is cancelled, then <c>false</c> is returned.
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns>True if "Yes" was chosen.</returns>
        bool YesOrNo(string queryText);

        /// <summary>
        /// Prompts the player to choose "Yes" or "No".
        /// If the prompt is cancelled, then <c>false</c> is returned.
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns>True if "Yes" was chosen.</returns>
        bool YesOrNo(YesOrNoOptions opts);

        /// <summary>
        /// Prompts the player to choose "Yes" or "No".
        /// If the prompt is cancelled, then <c>null</c> is returned.
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns>True if "Yes" was chosen.</returns>
        bool? YesOrNoOrCancel(string queryText);


        /// <summary>
        /// Prompts the player to choose "Yes" or "No".
        /// If the prompt is cancelled, then <c>null</c> is returned.
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns>True if "Yes" was chosen.</returns>
        bool? YesOrNoOrCancel(YesOrNoOptions opts);

        T? PickOrNone<T>(IEnumerable<T> choices, Prompt<T>.Args? opts = null) where T: class;
        T? PickOrNone<T>(IEnumerable<PromptChoice<T>> choices, Prompt<T>.Args? opts = null) where T: class;
        T? PickOrNoneS<T>(IEnumerable<T> choices, Prompt<T>.Args? opts = null) where T : struct;
        T? PickOrNoneS<T>(IEnumerable<PromptChoice<T>> choices, Prompt<T>.Args? opts = null) where T : struct;

        /// <summary>
        /// Shows a "More..." prompt and waits for the player to press a key.
        /// </summary>
        void PromptMore();
    }

    public class PlayerQuery : IPlayerQuery
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        private enum YesNo
        {
            Yes,
            No
        }

        public bool YesOrNo(string queryText) => YesOrNoOrCancel(queryText) ?? false;
        public bool YesOrNo(YesOrNoOptions opts) => YesOrNoOrCancel(opts) ?? false;

        public bool? YesOrNoOrCancel(string queryText)
        {
            return YesOrNoOrCancel(new YesOrNoOptions() { QueryText = queryText });
        }

        public bool? YesOrNoOrCancel(YesOrNoOptions opts)
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

            return null;
        }

        public T? PickOrNone<T>(IEnumerable<T> choices, Prompt<T>.Args? opts = null)
            where T: class
        {
            var items = choices.Select(c => new PromptChoice<T>(c));
            return PickOrNone(items, opts);
        }

        public T? PickOrNone<T>(IEnumerable<PromptChoice<T>> choices, Prompt<T>.Args? opts = null)
            where T : class
        {
            opts ??= new Prompt<T>.Args();
            opts.Choices = choices;

            var result = _uiManager.Query<Prompt<T>, Prompt<T>.Args, PromptChoice<T>>(opts);

            if (result.HasValue)
            {
                return result.Value.ChoiceData;
            }

            return null;
        }

        public T? PickOrNoneS<T>(IEnumerable<T> choices, Prompt<T>.Args? opts = null)
            where T : struct
        {
            var items = choices.Select(c => new PromptChoice<T>(c));
            return PickOrNoneS(items, opts);
        }

        public T? PickOrNoneS<T>(IEnumerable<PromptChoice<T>> choices, Prompt<T>.Args? opts = null)
            where T : struct
        {
            opts ??= new Prompt<T>.Args();
            opts.Choices = choices;

            var result = _uiManager.Query<Prompt<T>, Prompt<T>.Args, PromptChoice<T>>(opts);

            if (result.HasValue)
            {
                return result.Value.ChoiceData;
            }

            return null;
        }

        public void PromptMore()
        {
            _uiManager.Query<MorePrompt>();
        }
    }
}
