using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Layer
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
                UpdateText();
            }
        }

        private int _MaxValue;
        public int MaxValue
        {
            get => _MaxValue;
            set
            {
                _MaxValue = value;
                UpdateText();
            }
        }

        private int _Value;

        public int Value
        {
            get => _Value;
            set
            {
                _Value = value;
                UpdateText();
            }
        }

        public bool IsCancellable { get; set; }

        protected UiTopicWindow TopicWindow;

        protected IAssetDrawable AssetLabelInput;
        protected IAssetDrawable AssetArrowLeft;
        protected IAssetDrawable AssetArrowRight;

        protected Color ColorPromptBackground = UiColors.PromptBackground;
        protected FontSpec FontPromptText = UiFonts.PromptText;
        protected IUiText Text;

        public NumberPrompt(int maxValue = 1, int minValue = 1, int? initialValue = null, bool isCancellable = true)
        {
            _MinValue = minValue;
            _MaxValue = maxValue;
            if (initialValue == null)
                initialValue = MaxValue;

            initialValue = Math.Clamp(initialValue.Value, MinValue, MaxValue);

            _Value = initialValue.Value;
            IsCancellable = isCancellable;

            Text = new UiText(FontPromptText);

            AssetLabelInput = Assets.Get(AssetPrototypeOf.LabelInput);
            AssetArrowLeft = Assets.Get(AssetPrototypeOf.ArrowLeft);
            AssetArrowRight = Assets.Get(AssetPrototypeOf.ArrowRight);

            TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Two);

            UpdateText();

            BindKeys();
        }

        protected virtual void BindKeys()
        {
            Keybinds[CoreKeybinds.UIUp] += (_) =>
            {
                Value = MaxValue;
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            Keybinds[CoreKeybinds.UIDown] += (_) =>
            {
                Value = MinValue;
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            Keybinds[CoreKeybinds.UILeft] += (_) =>
            {
                Value = Math.Max(Value - 1, MinValue);
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            Keybinds[CoreKeybinds.UIRight] += (_) =>
            {
                Value = Math.Min(Value + 1, MaxValue);
                Sounds.Play(SoundPrototypeOf.Cursor1);
            };
            Keybinds[CoreKeybinds.Cancel] += (_) => { if (IsCancellable) Cancel(); };
            Keybinds[CoreKeybinds.Escape] += (_) => { if (IsCancellable) Cancel(); };
            Keybinds[CoreKeybinds.Enter] += (_) => Finish(new NumberPromptResult(Value));
        }

        public override void OnQuery()
        {
            Sounds.Play(SoundPrototypeOf.Pop2);
        }

        protected virtual void UpdateText()
        {
            Text.Text = $"{Value}({MaxValue})";
        }

        public const int DEFAULT_WIDTH = 8 * 16 + 60;
        public const int DEFAULT_HEIGHT = 36;

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(DEFAULT_WIDTH, DEFAULT_HEIGHT, out bounds);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            TopicWindow.SetSize(Width - 40, Height);
            Text.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            TopicWindow.SetPosition(X + 20, Y);
            Text.SetPosition(X + Width - 70 - Text.Width + 8, Y + 11);
        }

        public override void Update(float dt)
        {
            TopicWindow.Update(dt);
            Text.Update(dt);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(ColorPromptBackground);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, X + 24, Y + 4, Width - 42, Height - 1);

            TopicWindow.Draw();

            GraphicsEx.SetColor(Love.Color.White);
            AssetLabelInput.Draw(X + Width / 2 - 56, Y - 32);
            AssetArrowLeft.Draw(X + 28, Y + 4);
            AssetArrowRight.Draw(X + Width - 51, Y + 4);

            Text.Draw();
        }

        public override void Dispose()
        {
            TopicWindow.Dispose();
            Text.Dispose();
        }
    }
}
