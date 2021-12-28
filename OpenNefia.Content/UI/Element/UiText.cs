using OpenNefia.Core;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    public class UiText : BaseUiElement, IUiText
    {
        protected Love.Text BakedText;

        public int TextWidth { get => this.Font.LoveFont.GetWidth(this.Text); }

        private FontSpec _font;
        public FontSpec Font
        {
            get => _font;
            set
            {
                this._font = value;
                this.RebakeText();
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                this._text = value;
                this.RebakeText();
            }
        }

        public Color Color { get; set; }
        public Color BgColor { get; set; }

#pragma warning disable CS8618

        public UiText(string text = "") : this(new FontSpec(), text) { }

        public UiText(FontSpec font, string text = "")
        {
            this._text = text;
            this._font = font ?? throw new ArgumentNullException(nameof(font));
            this.Color = font.Color;
            this.BgColor = font.BgColor;
            this.RebakeText();
        }

#pragma warning restore CS8618

        public void RebakeText()
        {
            GraphicsEx.SetColor(this.Color);
            this.BakedText = Love.Graphics.NewText(this.Font.LoveFont, this.Text);
            this.SetPreferredSize();
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size.X = this.Font.LoveFont.GetWidth(this.Text);
            size.Y = this.Font.LoveFont.GetHeight() * this.Text.Split('\n').Length;
        }

        public override void Localize(LocaleKey key)
        {
            this.Text = Loc.Get(key);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(this.Color);
            Love.Graphics.Draw(this.BakedText, this.X, this.Y);
        }

        public override void Dispose()
        {
            this.BakedText.Dispose();
        }
    }

    public class UiTextOutlined : UiText
    {
        public UiTextOutlined(string text = "") : base(text) { }
        public UiTextOutlined(FontSpec font, string text = "") : base(font, text) { }

        public override void Draw()
        {
            GraphicsEx.SetColor(this.BgColor);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    Love.Graphics.Draw(this.BakedText, this.X + dx, this.Y + dy);

            GraphicsEx.SetColor(this.Color);
            Love.Graphics.Draw(this.BakedText, this.X, this.Y);
        }
    }

    public class UiTextShadowed : UiText
    {
        public UiTextShadowed(string text = "") : base(text) { }
        public UiTextShadowed(FontSpec font, string text = "") : base(font, text) { }

        public override void Draw()
        {
            GraphicsEx.SetColor(this.BgColor);
            Love.Graphics.Draw(this.BakedText, this.X + 1, this.Y + 1);

            GraphicsEx.SetColor(this.Color);
            Love.Graphics.Draw(this.BakedText, this.X, this.Y);
        }
    }

    public class UiTextTopic : UiText
    {
        private IAssetInstance AssetTipIcons;

        public UiTextTopic(string text = "") 
            : base(new FontSpec(12, 12, style: FontStyle.Bold), text)
        {
            AssetTipIcons = Assets.Get(AssetPrototypeOf.TipIcons);
        }

        public UiTextTopic(FontSpec font, string text = "") : base(font, text)
        {
            AssetTipIcons = Assets.Get(AssetPrototypeOf.TipIcons);
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            base.GetPreferredSize(out size);
            size.X += 26;
            size.Y += 8;
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);
            AssetTipIcons.DrawRegion("1", X, Y + 7);
            Love.Graphics.SetColor(Color);
            Love.Graphics.Draw(BakedText, X + 26, Y + 8); // y + vfix + 8
            Love.Graphics.SetColor(Color.Black);
            Love.Graphics.Line(X + 22, Y + 21, X + BakedText.GetWidth() + 36, Y + 21);
        }
    }
}
