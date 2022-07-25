using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.UI;
using OpenNefia.Core;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Logic;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.UI.Layer
{
    public class NumberPrompt : UiLayerWithResult<NumberPrompt.Args, NumberPrompt.Result>
    {
        public new class Args
        {
            public Args(int maxValue = 1, int minValue = 1, int? initialValue = null, bool isCancellable = true, string? prompt = null)
            {
                MaxValue = maxValue;
                MinValue = minValue;
                InitialValue = initialValue;
                IsCancellable = isCancellable;
                Prompt = prompt;
            }

            public int MaxValue { get; set; }
            public int MinValue { get; set; }
            public int? InitialValue { get; set; }
            public bool IsCancellable { get; set; }
            public string? Prompt { get; set; }
        }

        public new class Result
        {
            public int Value = 0;

            public Result(int value)
            {
                Value = value;
            }
        }

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

        private string? _Prompt;

        public bool IsCancellable { get; set; }

        [Dependency] private readonly IMessagesManager _mes = default!;

        protected IAssetInstance AssetLabelInput;
        protected IAssetInstance AssetArrowLeft;
        protected IAssetInstance AssetArrowRight;

        protected Color ColorPromptBackground = UiColors.PromptBackground;
        protected FontSpec FontPromptText = UiFonts.PromptText;

        [Child] protected UiText Text;
        [Child] protected UiTopicWindow TopicWindow;

        public NumberPrompt()
        {
            Text = new UiText(FontPromptText);

            AssetLabelInput = Assets.Get(Protos.Asset.LabelInput);
            AssetArrowLeft = Assets.Get(Protos.Asset.ArrowLeft);
            AssetArrowRight = Assets.Get(Protos.Asset.ArrowRight);

            TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Two);

            OnKeyBindDown += HandleKeyBindDown;
            CanControlFocus = true;
        }

        public override void Initialize(Args args)
        {
            base.Initialize(args);

            _MinValue = args.MinValue;
            _MaxValue = args.MaxValue;
            _Value = Math.Clamp(args.InitialValue ?? MaxValue, MinValue, MaxValue);
            _Prompt = args.Prompt;
            IsCancellable = args.IsCancellable;

            UpdateText();
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UIUp)
            {
                Value = MaxValue;
                Sounds.Play(Sound.Cursor1);
            }
            else if (args.Function == EngineKeyFunctions.UIDown)
            {
                Value = MinValue;
                Sounds.Play(Sound.Cursor1);
            }
            else if (args.Function == EngineKeyFunctions.UILeft)
            {
                Value = Math.Max(Value - 1, MinValue);
                Sounds.Play(Sound.Cursor1);
            }
            else if (args.Function == EngineKeyFunctions.UIRight)
            {
                Value = Math.Min(Value + 1, MaxValue);
                Sounds.Play(Sound.Cursor1);
            }
            else if (args.Function == EngineKeyFunctions.UICancel)
            {
                if (IsCancellable)
                    Cancel();
            }
            else if (args.Function == EngineKeyFunctions.UISelect)
            {
                Finish(new Result(Value));
            }
        }

        public override void OnQuery()
        {
            Sounds.Play(Sound.Pop2);

            if (_Prompt != null)
                _mes.Display(_Prompt);
        }

        protected virtual void UpdateText()
        {
            Text.Text = $"{Value}({MaxValue})";
        }

        public const int DEFAULT_WIDTH = 8 * 16 + 60;
        public const int DEFAULT_HEIGHT = 36;

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(DEFAULT_WIDTH, DEFAULT_HEIGHT, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            TopicWindow.SetSize(Width - 40, Height);
            Text.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
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
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + 24, Y + 4, Width - 42, Height - 1);

            TopicWindow.Draw();

            GraphicsEx.SetColor(Love.Color.White);
            AssetLabelInput.Draw(UIScale, X + Width / 2 - 56, Y - 32);
            AssetArrowLeft.Draw(UIScale, X + 28, Y + 4);
            AssetArrowRight.Draw(UIScale, X + Width - 51, Y + 4);

            Text.Draw();
        }

        public override void Dispose()
        {
            TopicWindow.Dispose();
            Text.Dispose();
        }
    }
}
