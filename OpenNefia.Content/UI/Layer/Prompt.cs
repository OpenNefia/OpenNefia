using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer
{
    public class PromptChoice<T> : IUiListItem
    {
        public T ChoiceData;
        public string? ChoiceText = null;
        public uint? ChoiceIndex = null;
        public Keys Key = Keys.None;

        public PromptChoice(T result, string? text = null, Keys key = Keys.None)
        {
            ChoiceData = result;
            ChoiceText = text;
            Key = key;
        }

        public string GetChoiceText(int index)
        {
            if (ChoiceText != null)
                return ChoiceText;

            return $"{ChoiceData}";
        }

        public UiListChoiceKey? GetChoiceKey(int index)
        {
            if (Key != Keys.None)
                return new UiListChoiceKey(Key, useKeybind: false);

            return UiListChoiceKey.MakeDefault(index);
        }
    }

    public struct PromptOptions
    {
        public int Width = 160;
        public bool IsCancellable = true;
        public string? QueryText = null;
    }

    public class Prompt<T> : BaseUiLayer<PromptChoice<T>>
    {
        private PromptOptions Options;

        public UiList<PromptChoice<T>> List { get; }
        public UiTopicWindow Window { get; }

        private int DefaultWidth;

        public Prompt(IEnumerable<PromptChoice<T>> choices, PromptOptions options)
        {
            List = new UiList<PromptChoice<T>>(choices);
            Window = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Zero);
            Options = options;

            DefaultWidth = Options.Width;

            BindKeys();
        }

        public Prompt(IEnumerable<PromptChoice<T>> choices)
            : this(choices, new PromptOptions())
        {
        }

        public Prompt(IEnumerable<T> choices, PromptOptions options)
            : this(choices.Select(x => new PromptChoice<T>(x)), options)
        {
        }

        public Prompt(IEnumerable<T> choices)
            : this(choices, new PromptOptions())
        {
        }

        protected virtual void BindKeys()
        {
            Action<UiKeyInputEventArgs> cancel = (_) =>
            {
                if (Options.IsCancellable)
                    Cancel();
            };

            Keybinds[CoreKeybinds.Cancel] += cancel;
            Keybinds[CoreKeybinds.Escape] += cancel;

            Forwards += List;

            List.EventOnActivate += (o, e) =>
            {
                Finish(e.SelectedCell.Data);
            };
        }

        public override void OnQuery()
        {
            if (Options.QueryText != null)
            {
                Mes.Display(Options.QueryText);
            }
            Sounds.Play(SoundPrototypeOf.Pop2);
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            List.GetPreferredSize(out var listSize);
            var width = Math.Max(DefaultWidth, listSize.X + 26 + 44);
            var height = listSize.Y;

            var promptX = (Love.Graphics.GetWidth() - 10) / 2 + 3;
            var promptY = (Love.Graphics.GetHeight() - Constants.INF_VERH - 30) / 2 - 4;

            var x = promptX - width / 2;
            var y = promptY - height / 2;

            bounds = UIBox2i.FromDimensions(x, y, width, height);
        }

        public override void SetSize(int width, int height)
        {
            List.SetSize(width, height);

            base.SetSize(width, height + 42);

            Window.SetSize(Width - 16, Height - 16);
        }

        public override void SetPosition(int x, int y)
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
