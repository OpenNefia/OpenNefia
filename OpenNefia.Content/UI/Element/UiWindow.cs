using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.UI.Element
{
    public class UiWindow : UiElement
    {
        public bool HasShadow { get; }

        private IList<UiKeyHint> _keyHints = new List<UiKeyHint>();
        public IList<UiKeyHint> KeyHints
        {
            get => _keyHints;
            set
            {
                _keyHints = value;
                RebuildKeyHintText();
            }
        }

        public int KeyHintXOffset { get; set; }
        public int YOffset { get; }
        public bool HasTitle => TextTitle.Text != string.Empty;
        public string Title { 
            get => TextTitle.Text;
            set
            {
                TextTitle.Text = value;

                // Update topic window position/size
                SetSize(Width, Height);
                SetPosition(X, Y);
            }
        }

        protected Color ColorBottomLine1 = UiColors.WindowBottomLine1;
        protected Color ColorBottomLine2 = UiColors.WindowBottomLine2;
        protected FontSpec FontWindowTitle = UiFonts.WindowTitle;
        protected FontSpec FontWindowKeyHints = UiFonts.WindowKeyHints;

        protected IAssetInstance AssetTipIcons;

        [Localize("Title")]
        protected UiText TextTitle;

        protected UiText TextKeyHint;

        protected UiWindowBacking Window;
        protected UiWindowBacking WindowShadow;
        protected UiTopicWindow TopicWindow;

        public UiWindow(bool hasShadow = true, List<UiKeyHint>? keyHints = null, int keyHintXOffset = 0, int yOffset = 0)
        {
            if (keyHints == null)
                keyHints = new List<UiKeyHint>();

            HasShadow = hasShadow;
            _keyHints = keyHints;
            KeyHintXOffset = keyHintXOffset;
            YOffset = yOffset;

            AssetTipIcons = Assets.Get(Protos.Asset.TipIcons);

            TextTitle = new UiTextOutlined(FontWindowTitle);
            TextKeyHint = new UiText(FontWindowKeyHints);

            Window = new UiWindowBacking();
            WindowShadow = new UiWindowBacking(UiWindowBacking.WindowBackingType.Shadow);
            TopicWindow = new UiTopicWindow();

            AddChild(TextTitle);
            AddChild(TextKeyHint);

            RebuildKeyHintText();
        }

        private void RebuildKeyHintText()
        {
            TextKeyHint.Text = UserInterfaceManager.FormatKeyHints(KeyHints);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            if (HasShadow)
                WindowShadow.SetPosition(X + 4, Y + 4);

            Window.SetPosition(x, y);

            if (HasTitle)
            {
                TextTitle.SetPosition(x + 45 * Width / 200 + 34 - TextTitle.Width / 2 
                    + Math.Clamp(TextTitle.Width - 120, 0, 200) / 2, Y + 4);
                TopicWindow.SetPosition(x + 34, y - 4);
            }

            TextKeyHint.SetPosition(x + 58 + KeyHintXOffset, y + Height - 43 - Height % 8);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            if (HasShadow)
                WindowShadow.SetSize(Width, Height);

            Window.SetSize(Width, Height);

            if (HasTitle)
            {
                TextTitle.SetPreferredSize();
                TopicWindow.SetSize(45 * Width / 100 + Math.Clamp(TextTitle.Width - 120, 0, 200), 32);
            }

            TextKeyHint.SetPreferredSize();
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            WindowShadow.Update(dt);
            TopicWindow.Update(dt);
            TextTitle?.Update(dt);
            TextKeyHint.Update(dt);
        }

        public override void Draw()
        {
            if (HasShadow)
            {
                GraphicsEx.SetColor(255, 255, 255, 80);
                Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                WindowShadow.Draw();
                Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            }

            GraphicsEx.SetColor(Color.White);
            Window.Draw();

            AssetTipIcons.DrawRegion("1", X + 30 + KeyHintXOffset, Y + Height - 47 - Height % 8);

            if (HasTitle)
            {
                TopicWindow.Draw();
                TextTitle.Draw();
            }

            GraphicsEx.SetColor(ColorBottomLine1);
            Love.Graphics.Line(
                X + 50 + KeyHintXOffset,
                Y + Height - 48 - Height % 8,
                X + Width - 40,
                Y + Height - 48 - Height % 8);

            GraphicsEx.SetColor(ColorBottomLine2);
            Love.Graphics.Line(
                X + 50 + KeyHintXOffset,
                Y + Height - 49 - Height % 8,
                X + Width - 40,
                Y + Height - 49 - Height % 8);

            TextKeyHint.Draw();
        }

        public override void Dispose()
        {
            TextTitle?.Dispose();
            TextKeyHint.Dispose();
            Window.Dispose();
            WindowShadow.Dispose();
            TopicWindow.Dispose();
        }
    }
}
