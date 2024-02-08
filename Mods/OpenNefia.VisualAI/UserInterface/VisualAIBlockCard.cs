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
        [Child] public UiWrappedText Text { get; }
        [Child] public UiText IndexText { get; }
        [Child] public UiTopicWindow TopicWindow { get; } = new(UiTopicWindow.FrameStyleKind.Four, UiTopicWindow.WindowStyleKind.One);
        [Child] public VisualAIBlockIcon Icon { get; }

        public bool IsSelected { get => Icon.IsSelected; set => Icon.IsSelected = value; }

        private Vector2i TextOffset => string.IsNullOrEmpty(IndexText.Text) ? Vector2i.Zero : (20, 0);

        public VisualAIBlockCard(string text = "", Color? color = null, PrototypeId<AssetPrototype>? icon = null, string? indexText = null, bool isSelected = true)
        {
            color ??= Color.White;

            var font = new FontSpec(13, 13, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
            Text = new UiWrappedText(new UiTextOutlined(font, text));
            Icon = new VisualAIBlockIcon(icon, color, isSelected);

            var indexFont = new FontSpec(20, 20, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
            if (indexText != null)
            {
                IndexText = new UiTextOutlined(indexFont, indexText);
            }
            else
            {
                IndexText = new UiTextOutlined(indexFont, "");
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            TopicWindow.SetSize(Width - 10, Height - 2);
            Text.SetSize(Width - 40 - 100 - TextOffset.X, Height - 12);
            IndexText.SetPreferredSize();
            Icon.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            TopicWindow.SetPosition(X + 5, y + 2);
            Text.SetPosition(X + 24 + TextOffset.X + Icon.Width + 5, Y + 10);
            IndexText.SetPosition(X + 20, Y + Height / 2 - IndexText.Height / 2);
            Icon.SetPosition(X + 20 + TextOffset.X, Y + Height / 2 - Icon.Height / 2 + TextOffset.Y);
        }

        public override void Draw()
        {
            TopicWindow.Draw();
            IndexText.Draw();
            Icon.Draw();
            Text.Draw();

            if (!IsSelected)
            {
                Love.Graphics.SetColor(Color.Black.WithAlphaB(64));
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, TopicWindow.Rect);
            }
        }

        public override void Update(float dt)
        {
            TopicWindow.Update(dt);
            Text.Update(dt);
            IndexText.Update(dt);
        }
    }
}
