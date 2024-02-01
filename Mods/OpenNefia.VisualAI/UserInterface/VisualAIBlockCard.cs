using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIBlockCard : UiElement
    {
        [Child] private UiWrappedText _text;
        [Child] private UiText _indexText;
        [Child] private UiTopicWindow _topicWindow = new(UiTopicWindow.FrameStyleKind.Four, UiTopicWindow.WindowStyleKind.One);
        [Child] private VisualAIBlockIcon _icon;

        public bool IsSelected { get => _icon.IsSelected; set => _icon.IsSelected = value; }

        private Color _iconColor;
        private IAssetInstance? _assetIcon;
        private Vector2 _tilePos;

        private Vector2i _textOffset => string.IsNullOrEmpty(_indexText.Text) ? Vector2i.Zero : (20, 0);

        public VisualAIBlockCard(string text, Color color, PrototypeId<AssetPrototype>? icon, string? indexText = null, bool isSelected = true)
        {
            var font = new FontSpec(13, 13, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
            _text = new UiWrappedText(new UiTextOutlined(font, text));
            _icon = new VisualAIBlockIcon(icon, color, isSelected);

            var indexFont = new FontSpec(20, 20, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
            if (indexText != null)
            {
                _indexText = new UiTextOutlined(indexFont, indexText);
            }
            else
            {
                _indexText = new UiTextOutlined(indexFont, "");
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _topicWindow.SetSize(Width - 10, Height - 2);
            _text.SetSize(Width - 40 - 100 - _textOffset.X, Height - 12);
            _indexText.SetPreferredSize();
            _icon.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _topicWindow.SetPosition(X + 5, y + 2);
            _text.SetPosition(X + 24 + _textOffset.X + _icon.Width + 5, Y + 10);
            _indexText.SetPosition(X + 20, Y + Height / 2 - _indexText.Height / 2);
            _icon.SetPosition(X + 20 + _textOffset.X, Y + Height / 2 - _icon.Height / 2 + _textOffset.Y);
        }

        public override void Draw()
        {
            _topicWindow.Draw();
            _indexText.Draw();
            _icon.Draw();
            _text.Draw();

            if (!IsSelected)
            {
                Love.Graphics.SetColor(Color.Black.WithAlphaB(64));
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, _topicWindow.Rect);
            }
        }

        public override void Update(float dt)
        {
            _topicWindow.Update(dt);
            _text.Update(dt);
            _indexText.Update(dt);
        }
    }
}
