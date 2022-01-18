using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;

namespace OpenNefia.Content.UI.Layer
{
    public class TextPrompt : UiLayerWithResult<TextPrompt.Args, TextPrompt.Result>
    {
        public class Result
        {
            public string Text { get; set; }
            public Result(string text)
            {
                Text = text;
            }
        }
        public class Args
        {
            public bool IsCancellable { get; set; } = true;
            public string? InitialValue { get; set; } = null;
            public int? MaxLength { get; set; } = 16;
            public bool HasShadow { get; set; } = true;
            public string? QueryText { get; set; } = null;

            public Args(int? maxLength = 16, string? initialValue = null, bool isCancellable = true, 
                bool hasShadow = true, string? prompt = null)
            {
                MaxLength = maxLength;
                InitialValue = initialValue;
                IsCancellable = isCancellable;
                HasShadow = hasShadow;
            }
        }

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
        public int? MaxLength { get; set; }
        public bool HasShadow { get; set; }
        private string? _queryText;

        protected bool IsCutOff = false;
        protected float Dt = 0f;
        protected double CaretAlpha = 2;

        protected UiTopicWindow TopicWindow;
        protected UiText Text;

        protected IAssetInstance AssetLabelInput;
        protected IAssetInstance AssetImeStatusJapanese;
        protected IAssetInstance AssetImeStatusEnglish;
        protected IAssetInstance AssetImeStatusNone;
        protected IAssetInstance AssetInputCaret;

        protected Color ColorPromptBackground = UiColors.PromptBackground;
        protected FontSpec FontPromptText = UiFonts.PromptText;

        public TextPrompt()
        {
            _Value = string.Empty;

            AssetLabelInput = Assets.Get(Protos.Asset.LabelInput);
            AssetImeStatusJapanese = Assets.Get(Protos.Asset.ImeStatusJapanese);
            AssetImeStatusEnglish = Assets.Get(Protos.Asset.ImeStatusEnglish);
            AssetImeStatusNone = Assets.Get(Protos.Asset.ImeStatusNone);
            AssetInputCaret = Assets.Get(Protos.Asset.InputCaret);

            TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Two);
            Text = new UiText(FontPromptText);

            OnKeyBindDown += HandleKeyBindDown;
            CanControlFocus = true;
            CanKeyboardFocus = true;

            AddChild(TopicWindow);
            AddChild(Text);

            UpdateText();
        }

        public override void Initialize(Args args)
        {
            MaxLength = args.MaxLength;
            _Value = args.InitialValue ?? string.Empty;
            IsCancellable = args.IsCancellable;
            HasShadow = args.HasShadow;
            _queryText = args.QueryText;

        }

        public override void GrabFocus()
        {
            base.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.TextSubmit)
            {
                Finish(new(Value));
            }
            else if (args.Function == EngineKeyFunctions.TextReleaseFocus)
            {
                if (IsCancellable)
                    Cancel();
            }
            else if (args.Function == EngineKeyFunctions.TextBackspace)
            {
                if (Value.Length > 0)
                {
                    Value = Value.Remove(Value.Length - 1, 1);
                    UpdateText();
                }
            }
        }

        protected override void TextEntered(GUITextEventArgs args)
        {
            Value += args.AsRune;
            UpdateText();
        }

        public override void OnQuery()
        {
            Sounds.Play(Protos.Sound.Pop2);
            if (_queryText != null)
            {
                Mes.Display(_queryText);
            }
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

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(DEFAULT_WIDTH, DEFAULT_HEIGHT, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            width = Math.Max(width, FontPromptText.LoveFont.GetWidth(Value));

            base.SetSize(width, height);

            TopicWindow.SetSize(Width, Height);
            Text.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
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
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + 4, Y + 4, Width - 1, Height - 1);
            }

            TopicWindow.Draw();

            GraphicsEx.SetColor(Love.Color.White);
            AssetLabelInput.DrawS(UIScale, X + Width / 2 - 60, Y - 32);

            if (IsCutOff)
            {
                AssetImeStatusNone.DrawS(UIScale, X + 8, Y + 4);
            }
            else
            {
                AssetImeStatusEnglish.DrawS(UIScale, X + 8, Y + 4);
            }

            Text.Draw();

            GraphicsEx.SetColor(255, 255, 255, (int)CaretAlpha);
            AssetInputCaret.DrawS(UIScale, X + Text.Width + 34, Y + 5);
        }

        public override void Dispose()
        {
            Text.Dispose();
            TopicWindow.Dispose();
        }
    }
}
