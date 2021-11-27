using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Element
{
    public class UiWindow : BaseUiElement
    {
        public string? Title { get; }
        public bool HasShadow { get; }
        public List<UiKeyHint> KeyHints { get; }
        public int XOffset { get; }
        public int YOffset { get; }
        public bool HasTitle => TitleText.Text != string.Empty;

        protected AssetDrawable AssetTipIcons;
        protected ColorDef ColorWindowBottomLine1;
        protected ColorDef ColorWindowBottomLine2;
        protected FontDef FontWindowTitle;
        protected FontDef FontWindowKeyHints;

        [Localize(Key="Title")]
        protected IUiText TitleText;

        protected IUiText KeyHintText;
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

            this.AssetTipIcons = new AssetDrawable(AssetDefOf.TipIcons);
            this.ColorWindowBottomLine1 = ColorDefOf.WindowBottomLine1;
            this.ColorWindowBottomLine2 = ColorDefOf.WindowBottomLine2;
            this.FontWindowTitle = FontDefOf.WindowTitle;
            this.FontWindowKeyHints = FontDefOf.WindowKeyHints;

            this.TitleText = new UiText(this.FontWindowTitle);
            this.KeyHintText = new UiText(this.FontWindowKeyHints, "hogepiyo");
            this.Window = new UiWindowBacking();
            this.WindowShadow = new UiWindowBacking(UiWindowBacking.WindowBackingType.Shadow);
            this.TopicWindow = new UiTopicWindow();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            if (this.HasShadow)
                this.WindowShadow.SetPosition(this.X + 4, this.Y + 4);

            this.Window.SetPosition(x, y);

            if (this.HasTitle)
            {
                this.TopicWindow.SetPosition(x + 34, y - 4);
                this.TitleText.SetPosition(x + 45 * this.Width / 200 + 34 - this.TitleText.Width / 2 + Math.Clamp(this.TitleText.Width - 120, 0, 200), this.Y + 4);
            }

            this.KeyHintText.SetPosition(x + 58 + this.XOffset, y + this.Height - 43 - this.Height % 8);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            if (this.HasShadow)
                this.WindowShadow.SetSize(this.Width, this.Height);

            this.Window.SetSize(this.Width, this.Height);

            if (this.HasTitle)
            {
                this.TopicWindow.SetSize(45 * this.Width / 100 + Math.Clamp(this.TitleText.Width - 120, 0, 200), 32);
                this.TitleText.SetPreferredSize();
            }

            this.KeyHintText.SetPreferredSize();
        }

        public override void Update(float dt)
        {
            this.Window.Update(dt);
            this.WindowShadow.Update(dt);
            this.TopicWindow.Update(dt);
            this.TitleText?.Update(dt);
            this.KeyHintText.Update(dt);
        }

        public override void Draw()
        {
            if (this.HasShadow)
            {
                GraphicsEx.SetColor(255, 255, 255, 80);
                Love.Graphics.SetBlendMode(BlendMode.Subtract);
                this.WindowShadow.Draw();
                Love.Graphics.SetBlendMode(BlendMode.Alpha);
            }

            GraphicsEx.SetColor(Color.White);
            this.Window.Draw();

            this.AssetTipIcons.DrawRegion("1", this.X + 30 + this.XOffset, this.Y + this.Height - 47 - this.Height % 8);

            if (this.HasTitle)
            {
                this.TopicWindow.Draw();
                this.TitleText.Draw();
            }

            GraphicsEx.SetColor(this.ColorWindowBottomLine1);
            Love.Graphics.Line(
                this.X + 50 + this.XOffset,
                this.Y + this.Height - 48 - this.Height % 8,
                this.X + this.Width - 40,
                this.Y + this.Height - 48 - this.Height % 8);

            GraphicsEx.SetColor(this.ColorWindowBottomLine2);
            Love.Graphics.Line(
                this.X + 50 + this.XOffset,
                this.Y + this.Height - 49 - this.Height % 8,
                this.X + this.Width - 40,
                this.Y + this.Height - 49 - this.Height % 8);

            this.KeyHintText.Draw();
        }

        public override void Dispose()
        {
            this.AssetTipIcons.Dispose();
            this.TitleText?.Dispose();
            this.KeyHintText.Dispose();
            this.Window.Dispose();
            this.WindowShadow.Dispose();
            this.TopicWindow.Dispose();
        }
    }
}
