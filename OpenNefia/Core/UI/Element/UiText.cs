using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.UI.Element
{
    public class UiText : BaseUiElement, IUiText
    {
        private Love.Text BakedText;

        public int TextWidth { get => this.Font.LoveFont.GetWidth(this.Text); }

        private FontSpec _font;
        [UiStyled] public FontSpec Font
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

        [UiStyled] public Maths.Color Color { get; set; }

#pragma warning disable CS8618

        public UiText(string text = "") : this(new FontSpec(), text) { }

        public UiText(FontSpec font, string text = "")
        {
            this._text = text;
            this._font = font ?? throw new ArgumentNullException(nameof(font));
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
            this.Text = Loc.GetString(key);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            //switch(this.Font.Style)
            //{
                //case FontStyle.Normal:
                    Love.Graphics.Draw(this.BakedText, this.Left, this.Top);
            //        break;

            //    case FontStyle.Outlined:
            //        GraphicsEx.SetColor(this.Font.ExtraColors[FontSpec.ColorKinds.Background]);
            //        for (int dx = -1; dx <= 1; dx++)
            //            for (int dy = -1; dy <= 1; dy++)
            //                Love.Graphics.Draw(this.BakedText, this.Left + dx, this.Top + dy);

            //        GraphicsEx.SetColor(this.UsedColor);
            //        Love.Graphics.Draw(this.BakedText, this.Left, this.Top);
            //        break;

            //    case FontStyle.Shadowed:
            //        GraphicsEx.SetColor(this.Font.ExtraColors[FontSpec.ColorKinds.Background]);
            //        Love.Graphics.Draw(this.BakedText, this.Left + 1, this.Top + 1);

            //        GraphicsEx.SetColor(this.UsedColor);
            //        Love.Graphics.Draw(this.BakedText, this.Left, this.Top);
            //        break;
            //}
        }

        public override void Dispose()
        {
            this.BakedText.Dispose();
        }
    }
}
