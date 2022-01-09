using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.UI.Layer
{
    public class NumberPrompt : UiLayerWithResult<NumberPrompt.Args, NumberPrompt.Result>
    {
        public new class Args
        {
            public Args(int maxValue = 1, int minValue = 1, int? initialValue = null, bool isCancellable = true)
            {
                MaxValue = maxValue;
                MinValue = minValue;
                InitialValue = initialValue;
                IsCancellable = isCancellable;
            }

            public int MaxValue { get; set; }
            public int MinValue { get; set; }
            public int? InitialValue { get; set; }
            public bool IsCancellable { get; set; }
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

        public bool IsCancellable { get; set; }

        protected UiTopicWindow TopicWindow;

        protected IAssetInstance AssetLabelInput;
        protected IAssetInstance AssetArrowLeft;
        protected IAssetInstance AssetArrowRight;

        protected Color ColorPromptBackground = UiColors.PromptBackground;
        protected FontSpec FontPromptText = UiFonts.PromptText;
        protected IUiText Text;

        public NumberPrompt()
        {
            Text = new UiText(FontPromptText);

            AssetLabelInput = Assets.Get(Protos.Asset.LabelInput);
            AssetArrowLeft = Assets.Get(Protos.Asset.ArrowLeft);
            AssetArrowRight = Assets.Get(Protos.Asset.ArrowRight);

            TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Two);

            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void Initialize(Args args)
        {
            base.Initialize(args);

            _MinValue = args.MinValue;
            _MaxValue = args.MaxValue;
            _Value = Math.Clamp(args.InitialValue ?? MaxValue, MinValue, MaxValue);
            IsCancellable = args.IsCancellable;

            UpdateText();
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
