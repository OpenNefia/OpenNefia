using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Input;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.UI.Layer
{
    public class PromptChoice<T> : IUiListItem
    {
        public T ChoiceData;
        public string? ChoiceText;
        public uint? ChoiceIndex;
        public Keyboard.Key Key;

        public PromptChoice(T result, string? text = null, Keyboard.Key key = Keyboard.Key.Unknown)
        {
            ChoiceData = result;
            ChoiceText = text;
            Key = key;
        }

        public PromptChoice(T result, LocaleKey localeKey, Keyboard.Key key = Keyboard.Key.Unknown)
            : this(result, Loc.GetString(localeKey), key) 
        {
        }

        public string GetChoiceText(int index)
        {
            if (ChoiceText != null)
                return ChoiceText;

            return $"{ChoiceData}";
        }

        public UiListChoiceKey? GetChoiceKey(int index)
        {
            if (Key != Keyboard.Key.Unknown)
                return new UiListChoiceKey(Key, useKeybind: false);

            return UiListChoiceKey.MakeDefault(index);
        }
    }

    public class Prompt<T> : UiLayerWithResult<Prompt<T>.Args, PromptChoice<T>>
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public class Args
        {
            public IEnumerable<PromptChoice<T>> Choices = Enumerable.Empty<PromptChoice<T>>();
            public int Width = 160;
            public bool IsCancellable = true;
            public string? QueryText = null;

            public Args(IEnumerable<PromptChoice<T>> choices)
            {
                Choices = choices;
            }

            public Args(IEnumerable<T> choices)
                : this(choices.Select(x => new PromptChoice<T>(x)))
            {
            }
        }

        [Child] public UiList<PromptChoice<T>> List { get; }
        [Child] public UiTopicWindow Window { get; }

        public bool IsCancellable { get; private set; }
        public string? QueryText { get; private set; }
        private int DefaultWidth { get; set; }

        public Prompt()
        {
            List = new UiList<PromptChoice<T>>();
            Window = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Zero);

            OnKeyBindDown += HandleKeyBindDown;

            List.OnActivated += (o, e) =>
            {
                Finish(e.SelectedCell.Data);
            };

            EventFilter = UIEventFilterMode.Pass;
        }

        public override void Initialize(Args args)
        {
            List.CreateAndSetCells(args.Choices);
            DefaultWidth = args.Width;
            IsCancellable = args.IsCancellable;
            QueryText = args.QueryText;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                if (IsCancellable)
                    Cancel();
            }
        }

        public override void OnQuery()
        {
            if (QueryText != null)
            {
                _mes.Display(QueryText);
            }
            Sounds.Play(Protos.Sound.Pop2);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            List.GetPreferredSize(out var listSize);
            var width = Math.Max(DefaultWidth, listSize.X + 26 + 44);
            var height = listSize.Y;

            var promptX = (_graphics.WindowSize.X - 10) / 2 + 3;
            var promptY = (_graphics.WindowSize.Y - Constants.INF_VERH / UIScale - 30) / 2 - 4;

            var x = promptX - width / 2;
            var y = promptY - height / 2;

            bounds = UIBox2.FromDimensions(x, y, width, height);
        }

        public override void SetSize(float width, float height)
        {
            List.SetSize(width, height);

            base.SetSize(width, height + 42);

            Window.SetSize(Width - 16, Height - 16);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            List.SetPosition(X + 30, Y + 24);
            Window.SetPosition(X + 8, Y + 8);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();
            List.Draw();
        }

        public override void Dispose()
        {
            Window.Dispose();
            List.Dispose();
        }
    }
}
