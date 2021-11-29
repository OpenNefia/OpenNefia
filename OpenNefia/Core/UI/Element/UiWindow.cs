using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.UI.Element
{
    public class UiWindow : BaseUiElement
    {
        public string? Title { get; }
        public bool HasShadow { get; }
        public List<UiKeyHint> KeyHints { get; }
        public int XOffset { get; }
        public int YOffset { get; }
        public bool HasTitle => TextTitle.Text != string.Empty;

        [UiStyled] protected Color ColorBottomLine1;
        [UiStyled] protected Color ColorBottomLine2;
        [UiStyled] protected FontSpec FontWindowTitle = default!;
        [UiStyled] protected FontSpec FontWindowKeyHints = default!;

        protected AssetDrawable AssetTipIcons;

        [UiStyled]
        [Localize("Title")]
        protected IUiText TextTitle = default!;

        [UiStyled]
        protected IUiText TextKeyHint = default!;

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

            this.Window = new UiWindowBacking();
            this.WindowShadow = new UiWindowBacking(UiWindowBacking.WindowBackingType.Shadow);
            this.TopicWindow = new UiTopicWindow();
        }

        public override void Initialize()
        {
            this.TextKeyHint.Text = "hogepiyo";
        }

        public override void SetPosition(Vector2i pos)
        {
            base.SetPosition(pos);

            if (this.HasShadow)
                this.WindowShadow.SetPosition(this.Left + 4, this.Top + 4);

            this.Window.SetPosition(pos);

            if (this.HasTitle)
            {
                this.TopicWindow.SetPosition(pos.X + 34, pos.Y - 4);
                this.TextTitle.SetPosition(pos.X + 45 * this.Width / 200 + 34 - this.TextTitle.Width / 2 + Math.Clamp(this.TextTitle.Width - 120, 0, 200), this.Top + 4);
            }

            this.TextKeyHint.SetPosition(pos.X + 58 + this.XOffset, pos.Y + this.Height - 43 - this.Height % 8);
        }

        public override void SetSize(Vector2i size)
        {
            base.SetSize(size);

            if (this.HasShadow)
                this.WindowShadow.SetSize(this.Size);

            this.Window.SetSize(this.Size);

            if (this.HasTitle)
            {
                this.TopicWindow.SetSize(45 * this.Width / 100 + Math.Clamp(this.TextTitle.Width - 120, 0, 200), 32);
                this.TextTitle.SetPreferredSize();
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

            this.AssetTipIcons.DrawRegion("1", this.Left + 30 + this.XOffset, this.Top + this.Height - 47 - this.Height % 8);

            if (this.HasTitle)
            {
                this.TopicWindow.Draw();
                this.TextTitle.Draw();
            }

            GraphicsEx.SetColor(this.ColorBottomLine1);
            Love.Graphics.Line(
                this.Left + 50 + this.XOffset,
                this.Top + this.Height - 48 - this.Height % 8,
                this.Left + this.Width - 40,
                this.Top + this.Height - 48 - this.Height % 8);

            GraphicsEx.SetColor(this.ColorBottomLine2);
            Love.Graphics.Line(
                this.Left + 50 + this.XOffset,
                this.Top + this.Height - 49 - this.Height % 8,
                this.Left + this.Width - 40,
                this.Top + this.Height - 49 - this.Height % 8);

            this.TextKeyHint.Draw();
        }

        public override void Dispose()
        {
            this.AssetTipIcons.Dispose();
            this.TextTitle?.Dispose();
            this.TextKeyHint.Dispose();
            this.Window.Dispose();
            this.WindowShadow.Dispose();
            this.TopicWindow.Dispose();
        }
    }
}
