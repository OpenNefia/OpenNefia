using OpenNefia.Core.Data.Types;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Element.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class PromptChoice<T> : IUiListItem
    {
        public T ChoiceData;
        public string? ChoiceText = null;
        public uint? ChoiceIndex = null;
        public Keys Key = Keys.None;

        public PromptChoice(T result, string? text = null, Keys key = Keys.None)
        {
            this.ChoiceData = result;
            this.ChoiceText = text;
            this.Key = key;
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
            this.List = new UiList<PromptChoice<T>>(choices);
            this.Window = new UiTopicWindow(UiTopicWindow.FrameStyle.Zero, UiTopicWindow.WindowStyle.Zero);
            this.Options = options;

            this.DefaultWidth = this.Options.Width;

            this.BindKeys();
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
            Action<KeyInputEvent> cancel = (_) => {
                if (this.Options.IsCancellable)
                    this.Cancel();
            };

            this.Keybinds[Keybind.Entries.Cancel] += cancel;
            this.Keybinds[Keybind.Entries.Escape] += cancel;

            this.Forwards += this.List;

            this.List.EventOnActivate += (o, e) =>
            {
                Sounds.PlayOneShot(SoundDefOf.Ok1);
                this.Finish(e.SelectedCell.Data);
            };
        }

        public override void OnQuery()
        {
            if (this.Options.QueryText != null)
            {
                Messages.Print(this.Options.QueryText);
            }
            Sounds.PlayOneShot(SoundDefOf.Pop2);
        }

        public override void GetPreferredBounds(out int x, out int y, out int width, out int height)
        {
            this.List.GetPreferredSize(out var listWidth, out var listHeight);
            width = Math.Max(this.DefaultWidth, listWidth + 26 + 44);
            height = listHeight;

            var promptX = (Love.Graphics.GetWidth() - 10) / 2 + 3;
            var promptY = (Love.Graphics.GetHeight() - Constants.INF_VERH - 30) / 2 - 4;

            x = promptX - width / 2;
            y = promptY - height / 2;
        }

        public override void SetSize(int width = 0, int height = 0)
        {
            this.List.SetSize(width, height);

            base.SetSize(width, height + 42);

            this.Window.SetSize(this.Width - 16, this.Height - 16);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            this.List.SetPosition(this.X + 30, this.Y + 24);
            this.Window.SetPosition(this.X + 8, this.Y + 8);
        }

        public override void Update(float dt)
        {
            this.Window.Update(dt);
            this.List.Update(dt);
        }

        public override void Draw()
        {
            this.Window.Draw();
            this.List.Draw();
        }

        public override void Dispose()
        {
            this.Window.Dispose();
            this.List.Dispose();
        }
    }
}
