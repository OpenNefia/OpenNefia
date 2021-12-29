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
        public List<UiKeyHint> KeyHints { get; }
        public int XOffset { get; }
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

        public UiWindow(bool hasShadow = true, List<UiKeyHint>? keyHints = null, int xOffset = 0, int yOffset = 0)
        {
            if (keyHints == null)
                keyHints = new List<UiKeyHint>();

            this.HasShadow = hasShadow;
            this.KeyHints = keyHints;
            this.XOffset = xOffset;
            this.YOffset = yOffset;

            this.AssetTipIcons = Assets.Get(AssetPrototypeOf.TipIcons);

            this.TextTitle = new UiTextOutlined(this.FontWindowTitle);
            this.TextKeyHint = new UiText(this.FontWindowKeyHints, "(key hints)");

            this.Window = new UiWindowBacking();
            this.WindowShadow = new UiWindowBacking(UiWindowBacking.WindowBackingType.Shadow);
            this.TopicWindow = new UiTopicWindow();

            AddChild(TextTitle);
            AddChild(TextKeyHint);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            if (this.HasShadow)
                this.WindowShadow.SetPosition(this.X + 4, this.Y + 4);

            this.Window.SetPosition(x, y);

            if (this.HasTitle)
            {
                this.TextTitle.SetPosition(x + 45 * this.Width / 200 + 34 - this.TextTitle.Width / 2 
                    + Math.Clamp(this.TextTitle.Width - 120, 0, 200) / 2, this.Y + 4);
                this.TopicWindow.SetPosition(x + 34, y - 4);
            }

            this.TextKeyHint.SetPosition(x + 58 + this.XOffset, y + this.Height - 43 - this.Height % 8);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            if (this.HasShadow)
                this.WindowShadow.SetSize(this.Width, this.Height);

            this.Window.SetSize(this.Width, this.Height);

            if (this.HasTitle)
            {
                this.TextTitle.SetPreferredSize();
                this.TopicWindow.SetSize(45 * this.Width / 100 + Math.Clamp(this.TextTitle.Width - 120, 0, 200), 32);
            }

            this.TextKeyHint.SetPreferredSize();
        }

        public override void Update(float dt)
        {
            this.Window.Update(dt);
            this.WindowShadow.Update(dt);
            this.TopicWindow.Update(dt);
            this.TextTitle?.Update(dt);
            this.TextKeyHint.Update(dt);
        }

        public override void Draw()
        {
            if (this.HasShadow)
            {
                GraphicsEx.SetColor(255, 255, 255, 80);
                Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                this.WindowShadow.Draw();
                Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            }

            GraphicsEx.SetColor(Color.White);
            this.Window.Draw();

            this.AssetTipIcons.DrawRegion("1", this.X + 30 + this.XOffset, this.Y + this.Height - 47 - this.Height % 8);

            if (this.HasTitle)
            {
                this.TopicWindow.Draw();
                this.TextTitle.Draw();
            }

            GraphicsEx.SetColor(this.ColorBottomLine1);
            Love.Graphics.Line(
                this.X + 50 + this.XOffset,
                this.Y + this.Height - 48 - this.Height % 8,
                this.X + this.Width - 40,
                this.Y + this.Height - 48 - this.Height % 8);

            GraphicsEx.SetColor(this.ColorBottomLine2);
            Love.Graphics.Line(
                this.X + 50 + this.XOffset,
                this.Y + this.Height - 49 - this.Height % 8,
                this.X + this.Width - 40,
                this.Y + this.Height - 49 - this.Height % 8);

            this.TextKeyHint.Draw();
        }

        public override void Dispose()
        {
            this.TextTitle?.Dispose();
            this.TextKeyHint.Dispose();
            this.Window.Dispose();
            this.WindowShadow.Dispose();
            this.TopicWindow.Dispose();
        }
    }
}
