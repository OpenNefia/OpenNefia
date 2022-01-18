using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    public class UiText : UiElement
    {
        protected Love.Text BakedText;

        /// <inheritdoc/>
        public float TextWidth => TextPixelWidth / UIScale;

        /// <inheritdoc/>
        public int TextPixelWidth { get => Font.LoveFont.GetWidth(Text); }

        private FontSpec _font;
        public FontSpec Font
        {
            get => _font;
            set
            {
                _font = value;
                RebakeText();
            }
        }

        protected string _text;
        public virtual string Text
        {
            get => _text;
            set
            {
                _text = value;
                RebakeText();
            }
        }

        public Color Color { get; set; }
        public Color BgColor { get; set; }

        public UiText(string text = "") : this(new FontSpec(), text) { }

        public UiText(FontSpec font, string text = "")
        {
            _text = text;
            _font = font ?? throw new ArgumentNullException(nameof(font));
            Color = font.Color;
            BgColor = font.BgColor;
            BakedText = Love.Graphics.NewText(Font.LoveFont, string.Empty);
            RebakeText();
        }

        protected override void UIScaleChanged(GUIScaleChangedEventArgs args)
        {
            ReallocateText();
        }

        private void ReallocateText()
        {
            BakedText.Dispose();
            BakedText = Love.Graphics.NewText(Font.LoveFont, _text);
        }

        public void RebakeText()
        {
            BakedText.Set(_text, Color.White);
            SetPreferredSize();
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size.X = Font.LoveFont.GetWidthV(UIScale, Text);
            size.Y = Font.LoveFont.GetHeightV(UIScale);
        }

        public override void Localize(LocaleKey key)
        {
            Text = Loc.GetString(key);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Color);
            Love.Graphics.Draw(BakedText, PixelX, PixelY);
        }

        public override void Dispose()
        {
            BakedText.Dispose();
        }
    }

    public class UiTextOutlined : UiText
    {
        public UiTextOutlined(string text = "") : base(text) { }
        public UiTextOutlined(FontSpec font, string text = "") : base(font, text) { }

        public override void Draw()
        {
            GraphicsEx.SetColor(BgColor);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    Love.Graphics.Draw(BakedText, PixelX + dx, PixelY + dy);

            GraphicsEx.SetColor(Color);
            Love.Graphics.Draw(BakedText, PixelX, PixelY);
        }
    }

    public class UiTextShadowed : UiText
    {
        public UiTextShadowed(string text = "") : base(text) { }
        public UiTextShadowed(FontSpec font, string text = "") : base(font, text) { }

        public override void Draw()
        {
            GraphicsEx.SetColor(BgColor);
            Love.Graphics.Draw(BakedText, PixelX + 1, PixelY + 1);

            GraphicsEx.SetColor(Color);
            Love.Graphics.Draw(BakedText, PixelX, PixelY);
        }
    }

    public class UiTextTopic : UiText
    {
        private IAssetInstance AssetTipIcons;

        public UiTextTopic(string text = "") 
            : base(new FontSpec(12, 12, style: FontStyle.Bold), text)
        {
            AssetTipIcons = Assets.Get(Protos.Asset.TipIcons);
        }

        public UiTextTopic(FontSpec font, string text = "") : base(font, text)
        {
            AssetTipIcons = Assets.Get(Protos.Asset.TipIcons);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            base.GetPreferredSize(out size);
            size.X += 26;
            size.Y += 8;
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);
            AssetTipIcons.DrawRegionS(UIScale, "1", X, Y + 7);
            Love.Graphics.SetColor(Color);
            Love.Graphics.Draw(BakedText, PixelX + 26 * UIScale, PixelY + 8 * UIScale); // y + vfix + 8
            Love.Graphics.SetColor(Color.Black);
            GraphicsS.LineS(UIScale, X + 22, Y + 21, X + BakedText.GetWidthV(UIScale) + 36, Y + 21);
        }
    }
}
