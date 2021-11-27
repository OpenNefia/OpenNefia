using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using static OpenNefia.Core.Rendering.GraphicsEx;

namespace OpenNefia.Core.UI.Element
{
    public class UiText : BaseUiElement, IUiText
    {
        private Love.Text BakedText;

        public int TextWidth { get => this.Font.GetWidth(this.Text); }

        private FontDef _Font;
        public FontDef Font
        {
            get => _Font;
            set
            {
                this._Font = value;
                this.RebakeText();
            }
        }

        private string _Text;
        public string Text
        {
            get => _Text;
            set
            {
                this._Text = value;
                this.RebakeText();
            }
        }

        public Love.Color UsedColor { get; private set; }

        private Love.Color? _Color;
        public Love.Color? Color
        {
            get => UsedColor;
            set
            {
                this._Color = value;
                if (this._Color != null)
                    this.UsedColor = this._Color.Value;
                else
                    this.UsedColor = this.Font.Color;
            }
        }

#pragma warning disable CS8618
        
        public UiText(FontDef font, string text = "", Love.Color? color = null)
        {
            this._Text = text;
            this._Font = font;
            this.Color = color;
            this.RebakeText();
        }

#pragma warning restore CS8618

        public void RebakeText()
        {
            this.BakedText = Love.Graphics.NewText(this.Font, this.Text);
            this.SetPreferredSize();
        }

        public override void GetPreferredSize(out int width, out int height)
        {
            width = this.Font.GetWidth(this.Text);
            height = this.Font.GetHeight() * this.Text.Split('\n').Length;
        }

        public override void Localize(LocaleKey key)
        {
            this.Text = I18N.GetString(key);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            switch(this.Font.Style)
            {
                case FontStyle.Normal:
                    GraphicsEx.SetColor(this.UsedColor);
                    Love.Graphics.Draw(this.BakedText, this.X, this.Y);
                    break;

                case FontStyle.Outlined:
                    GraphicsEx.SetColor(this.Font.ExtraColors[FontDef.ColorKinds.Background]);
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                            Love.Graphics.Draw(this.BakedText, this.X + dx, this.Y + dy);

                    GraphicsEx.SetColor(this.UsedColor);
                    Love.Graphics.Draw(this.BakedText, this.X, this.Y);
                    break;

                case FontStyle.Shadowed:
                    GraphicsEx.SetColor(this.Font.ExtraColors[FontDef.ColorKinds.Background]);
                    Love.Graphics.Draw(this.BakedText, this.X + 1, this.Y + 1);

                    GraphicsEx.SetColor(this.UsedColor);
                    Love.Graphics.Draw(this.BakedText, this.X, this.Y);
                    break;
            }
        }

        public override void Dispose()
        {
            this.BakedText.Dispose();
        }
    }
}
