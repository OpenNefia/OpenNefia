using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

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

        protected IAssetDrawable AssetLabelInput;
        protected IAssetDrawable AssetArrowLeft;
        protected IAssetDrawable AssetArrowRight;

        protected Color ColorPromptBackground = UiColors.PromptBackground;
        protected IUiText Text;

        public NumberPrompt(int maxValue = 1, int minValue = 1, int? initialValue = null, bool isCancellable = true)
        {
            this._MinValue = minValue;
            this._MaxValue = maxValue;
            if (initialValue == null)
                initialValue = this.MaxValue;

            initialValue = Math.Clamp(initialValue.Value, this.MinValue, this.MaxValue);

            this._Value = initialValue.Value;
            this.IsCancellable = isCancellable;

            this.Text = new UiText();

            this.AssetLabelInput = Assets.Get(AssetPrototypeOf.LabelInput);
            this.AssetArrowLeft = Assets.Get(AssetPrototypeOf.ArrowLeft);
            this.AssetArrowRight = Assets.Get(AssetPrototypeOf.ArrowRight);

            this.TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Two);

            this.UpdateText();

            this.BindKeys();
        }

        protected virtual void BindKeys()
        {
            this.Keybinds[CoreKeybinds.UIUp] += (_) => {
                this.Value = this.MaxValue;
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            this.Keybinds[CoreKeybinds.UIDown] += (_) => {
                this.Value = this.MinValue;
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            this.Keybinds[CoreKeybinds.UILeft] += (_) => {
                this.Value = Math.Max(this.Value - 1, this.MinValue);
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            this.Keybinds[CoreKeybinds.UIRight] += (_) => {
                this.Value = Math.Min(this.Value + 1, this.MaxValue);
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            this.Keybinds[CoreKeybinds.Cancel] += (_) => { if (this.IsCancellable) this.Cancel(); };
            this.Keybinds[CoreKeybinds.Escape] += (_) => { if (this.IsCancellable) this.Cancel(); };
            this.Keybinds[CoreKeybinds.Enter] += (_) => this.Finish(new NumberPromptResult(this.Value));
        }

        public override void OnQuery()
        {
            Sounds.Play(SoundPrototypeOf.Pop2);
        }

        protected virtual void UpdateText()
        {
            this.Text.Text = $"{this.Value}({this.MaxValue})";
        }

        public const int DEFAULT_WIDTH = 8 * 16 + 60;
        public const int DEFAULT_HEIGHT = 36;

        public override void GetPreferredBounds(out Box2i bounds)
        {
            UiUtils.GetCenteredParams(DEFAULT_WIDTH, DEFAULT_HEIGHT, out bounds);
        }

        public override void SetSize(int width, int height)
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
