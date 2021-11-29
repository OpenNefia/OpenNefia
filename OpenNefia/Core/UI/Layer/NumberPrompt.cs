using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class NumberPromptResult
    {
        public int Value = 0;

        public NumberPromptResult(int value)
        {
            Value = value;
        }
    }

    public class NumberPrompt : BaseUiLayer<NumberPromptResult>
    {
        private int _MinValue;
        public int MinValue
        {
            get => _MinValue;
            set
            {
                _MinValue = value;
                this.UpdateText();
            }
        }

        private int _MaxValue;
        public int MaxValue { 
            get => _MaxValue; 
            set
            {
                _MaxValue = value;
                this.UpdateText();
            }
        }

        private int _Value;

        public int Value
        {
            get => _Value;
            set
            {
                _Value = value;
                this.UpdateText();
            }
        }

        public bool IsCancellable { get; set; }

        protected UiTopicWindow TopicWindow;
        protected IUiText Text;

        protected AssetDrawable AssetLabelInput;
        protected AssetDrawable AssetArrowLeft;
        protected AssetDrawable AssetArrowRight;
        protected ColorDef ColorPromptBackground;
        protected FontDef FontPromptText;

        public NumberPrompt(int maxValue = 1, int minValue = 1, int? initialValue = null, bool isCancellable = true)
        {
            this._MinValue = minValue;
            this._MaxValue = maxValue;
            if (initialValue == null)
                initialValue = this.MaxValue;

            initialValue = Math.Clamp(initialValue.Value, this.MinValue, this.MaxValue);

            this._Value = initialValue.Value;
            this.IsCancellable = isCancellable;

            this.AssetLabelInput = new AssetDrawable(AssetDefOf.LabelInput);
            this.AssetArrowLeft = new AssetDrawable(AssetDefOf.ArrowLeft);
            this.AssetArrowRight = new AssetDrawable(AssetDefOf.ArrowRight);
            this.ColorPromptBackground = ColorDefOf.PromptBackground;
            this.FontPromptText = FontDefOf.PromptText;

            this.TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyle.Zero, UiTopicWindow.WindowStyle.Two);
            this.Text = new UiText(this.FontPromptText);
            
            this.UpdateText();

            this.BindKeys();
        }

        protected virtual void BindKeys()
        {
            this.Keybinds[Keybind.UIUp] += (_) => {
                this.Value = this.MaxValue;
                Sounds.PlayOneShot(SoundDefOf.Cursor1);
            };
            this.Keybinds[Keybind.UIDown] += (_) => {
                this.Value = this.MinValue;
                Sounds.PlayOneShot(SoundDefOf.Cursor1);
            };
            this.Keybinds[Keybind.UILeft] += (_) => {
                this.Value = Math.Max(this.Value - 1, this.MinValue);
                Sounds.PlayOneShot(SoundDefOf.Cursor1);
            };
            this.Keybinds[Keybind.UIRight] += (_) => {
                this.Value = Math.Min(this.Value + 1, this.MaxValue);
                Sounds.PlayOneShot(SoundDefOf.Cursor1);
            };
            this.Keybinds[Keybind.Cancel] += (_) => { if (this.IsCancellable) this.Cancel(); };
            this.Keybinds[Keybind.Escape] += (_) => { if (this.IsCancellable) this.Cancel(); };
            this.Keybinds[Keybind.Enter] += (_) => this.Finish(new NumberPromptResult(this.Value));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override UiResult<NumberPromptResult> Query() => base.Query();

        public override void OnQuery()
        {
            Sounds.PlayOneShot(SoundDefOf.Pop2);
        }

        protected virtual void UpdateText()
        {
            this.Text.Text = $"{this.Value}({this.MaxValue})";
        }

        public const int DEFAULT_WIDTH = 8 * 16 + 60;
        public const int DEFAULT_HEIGHT = 36;

        public override void GetPreferredBounds(out int x, out int y, out int width, out int height)
        {
            var rect = UiUtils.GetCenteredParams(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            x = rect.X;
            y = rect.Y;
            width = rect.Width;
            height = rect.Height;
        }

        public override void SetSize(int width = 0, int height = 0)
        {
            base.SetSize(width, height);

            this.TopicWindow.SetSize(this.Width - 40, this.Height);
            this.Text.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            this.TopicWindow.SetPosition(this.X + 20, this.Y);
            this.Text.SetPosition(this.X + this.Width - 70 - Text.Width + 8, this.Y + 11);
        }

        public override void Update(float dt)
        {
            this.TopicWindow.Update(dt);
            this.Text.Update(dt);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(this.ColorPromptBackground);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.X + 24, this.Y + 4, this.Width - 42, this.Height - 1);
            
            this.TopicWindow.Draw();

            GraphicsEx.SetColor(Love.Color.White);
            this.AssetLabelInput.Draw(this.X + this.Width / 2 - 56, this.Y - 32);
            this.AssetArrowLeft.Draw(this.X + 28, this.Y + 4);
            this.AssetArrowRight.Draw(this.X + this.Width - 51, this.Y + 4);

            this.Text.Draw();
        }

        public override void Dispose()
        {
            this.TopicWindow.Dispose();
            this.AssetLabelInput.Dispose();
            this.AssetArrowLeft.Dispose();
            this.AssetArrowRight.Dispose();
            this.Text.Dispose();
        }
    }
}
