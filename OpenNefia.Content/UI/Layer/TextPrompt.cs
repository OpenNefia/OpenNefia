using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.UI.Layer
{
    public class TextPrompt : UiLayerWithResult<string>
    {
        private string _Value;
        public string Value
        {
            get => _Value;
            set
            {
                _Value = value;
                UpdateText();
            }
        }

        public bool IsCancellable { get; set; }
        public bool LimitLength { get; set; }
        public int? MaxLength { get; set; }
        public bool HasShadow { get; set; }

        protected bool IsCutOff = false;
        protected float Dt = 0f;
        protected double CaretAlpha = 2;

        protected UiTopicWindow TopicWindow;
        protected IUiText Text;

        protected IAssetInstance AssetLabelInput;
        protected IAssetInstance AssetImeStatusJapanese;
        protected IAssetInstance AssetImeStatusEnglish;
        protected IAssetInstance AssetImeStatusNone;
        protected IAssetInstance AssetInputCaret;

        protected Color ColorPromptBackground = UiColors.PromptBackground;
        protected FontSpec FontPromptText = UiFonts.PromptText;

        public TextPrompt(int? maxLength = 16, bool limitLength = false, string? initialValue = null, bool isCancellable = true, bool hasShadow = true)
        {
            if (initialValue == null)
                initialValue = string.Empty;

            MaxLength = maxLength;
            LimitLength = limitLength;
            _Value = initialValue;
            IsCancellable = isCancellable;
            HasShadow = hasShadow;

            AssetLabelInput = Assets.Get(AssetPrototypeOf.LabelInput);
            AssetImeStatusJapanese = Assets.Get(AssetPrototypeOf.ImeStatusJapanese);
            AssetImeStatusEnglish = Assets.Get(AssetPrototypeOf.ImeStatusEnglish);
            AssetImeStatusNone = Assets.Get(AssetPrototypeOf.ImeStatusNone);
            AssetInputCaret = Assets.Get(AssetPrototypeOf.InputCaret);

            TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Two);
            Text = new UiText(FontPromptText);

            UpdateText();

            BindKeys();
        }

        protected virtual void BindKeys()
        {
            //TextInput.Enabled = true;
            //TextInput.Callback += (evt) =>
            //{
            //    Value = Value + evt.Text;
            //    UpdateText();
            //};
            //Keybinds[CoreKeybinds.Enter] += (_) => Finish(Value);
            //Keybinds[CoreKeybinds.Escape] += (_) =>
            //{
            //    if (IsCancellable)
            //        Cancel();
            //};
            //Keybinds[Keys.Backspace] += (_) =>
            //{
            //    if (Value.Length > 0)
            //    {
            //        Value = Value.Remove(Value.Length - 1, 1);
            //        UpdateText();
            //    }
            //};
        }

        public override void OnQuery()
        {
            Sounds.Play(Protos.Sound.Pop2);
        }

        protected virtual void UpdateText()
        {
            IsCutOff = false;
            string displayText = string.Empty;
            var wideLength = Value.GetWideLength();

            if (MaxLength.HasValue)
            {
                if (MaxLength.HasValue && wideLength > MaxLength.Value - 2)
                {
                    var dots = "...";
                    if (Loc.IsFullwidth())
                    {
                        dots = "…";
                    }
                    displayText = Value.WideSubstring(0, MaxLength.Value - 2) + dots;
                }
                else
                {
                    displayText = Value;
                }
            }
            else
            {
                displayText = Value;
            }

            Text.Text = displayText;

            if (MaxLength.HasValue && wideLength > MaxLength)
            {
                _Value = Value.WideSubstring(0, MaxLength);
                IsCutOff = true;
            }
        }

        public const int DEFAULT_WIDTH = 16 * 16 + 60;
        public const int DEFAULT_HEIGHT = 36;

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(DEFAULT_WIDTH, DEFAULT_HEIGHT, out bounds);
        }

        public override void SetSize(int width, int height)
        {
            width = Math.Max(width, FontPromptText.LoveFont.GetWidth(Value));

            base.SetSize(width, height);

            TopicWindow.SetSize(Width, Height);
            Text.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            TopicWindow.SetPosition(X, Y);
            Text.SetPosition(X + 36, Y + 9);
        }

        public override void Update(float dt)
        {
            Dt += dt / Constants.SCREEN_REFRESH * 4;
            CaretAlpha = Math.Sin(Dt) * 255f * 1f;
            TopicWindow.Update(dt);
            Text.Update(dt);
        }

        public override void Draw()
        {
            if (HasShadow)
            {
                GraphicsEx.SetColor(ColorPromptBackground);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, X + 4, Y + 4, Width - 1, Height - 1);
            }

            TopicWindow.Draw();

            GraphicsEx.SetColor(Love.Color.White);
            AssetLabelInput.Draw(X + Width / 2 - 60, Y - 32);

            if (IsCutOff)
            {
                AssetImeStatusNone.Draw(X + 8, Y + 4);
            }
            else
            {
                AssetImeStatusEnglish.Draw(X + 8, Y + 4);
            }

            Text.Draw();

            GraphicsEx.SetColor(255, 255, 255, (int)CaretAlpha);
            AssetInputCaret.Draw(X + Text.Width + 34, Y + 5);
        }

        public override void Dispose()
        {
            Text.Dispose();
            TopicWindow.Dispose();
        }
    }
}
