using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Content.Prototypes.Protos;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.UI.Element
{
    public sealed class UiScroll : UiElement
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

        private Color ColorBottomLine1 = UiColors.ScrollBottomLine1;
        private Color ColorBottomLine2 = UiColors.ScrollBottomLine2;
        private FontSpec FontWindowTitle = UiFonts.WindowTitle;
        private FontSpec FontWindowKeyHints = UiFonts.WindowKeyHints;

        private IAssetInstance AssetTipIcons;

        [Child] private UiText TextKeyHint;
        [Child] private UiWindowBacking Scroll;
        [Child] private UiWindowBacking ScrollShadow;

        public UiScroll(bool hasShadow = true, List<UiKeyHint>? keyHints = null, int keyHintXOffset = 0, int yOffset = 0)
        {
            if (keyHints == null)
                keyHints = new List<UiKeyHint>();

            HasShadow = hasShadow;
            _keyHints = keyHints;
            KeyHintXOffset = keyHintXOffset;
            YOffset = yOffset;

            AssetTipIcons = Assets.Get(Asset.TipIcons);

            TextKeyHint = new UiText(FontWindowKeyHints);

            Scroll = new UiWindowBacking(Asset.IeScroll);
            ScrollShadow = new UiWindowBacking(Asset.IeScroll, UiWindowBacking.WindowBackingType.Shadow);

            RebuildKeyHintText();
        }

        private void RebuildKeyHintText()
        {
            TextKeyHint.Text = UserInterfaceManager.FormatKeyHints(KeyHints);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            if (HasShadow)
                ScrollShadow.SetSize(Width, Height);

            Scroll.SetSize(Width, Height);

            TextKeyHint.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            if (HasShadow)
                ScrollShadow.SetPosition(X + 4, Y + 4);

            Scroll.SetPosition(X, Y);

            TextKeyHint.SetPosition(X + 68 + KeyHintXOffset, Y + Height - 63 - Height % 8);
        }

        public override void Update(float dt)
        {
            Scroll.Update(dt);
            ScrollShadow.Update(dt);
            TextKeyHint.Update(dt);
        }

        public override void Draw()
        {
            if (HasShadow)
            {
                GraphicsEx.SetColor(255, 255, 255, 80);
                Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                ScrollShadow.Draw();
                Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            }

            GraphicsEx.SetColor(Color.White);
            Scroll.Draw();

            AssetTipIcons.DrawRegion(UIScale, "1", X + 40 + KeyHintXOffset, Y + Height - 67 - Height % 8);

            GraphicsEx.SetColor(ColorBottomLine1);
            GraphicsS.LineS(
                UIScale,
                X + 60 + KeyHintXOffset,
                Y + Height - 68 - Height % 8,
                X + Width - 40,
                Y + Height - 68 - Height % 8);

            GraphicsEx.SetColor(ColorBottomLine2);
            GraphicsS.LineS(
                UIScale,
                X + 60 + KeyHintXOffset,
                Y + Height - 69 - Height % 8,
                X + Width - 40,
                Y + Height - 69 - Height % 8);

            TextKeyHint.Draw();
        }

        public override void Dispose()
        {
            TextKeyHint.Dispose();
            Scroll.Dispose();
            ScrollShadow.Dispose();
        }
    }
}
