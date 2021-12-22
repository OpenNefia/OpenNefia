using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.UI.Hud
{
    public class ColoredString
    {
        public readonly string Text;
        public readonly Color Color;

        public ColoredString(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }

    public class SimpleMessageWindow : BaseUiElement, IHudMessageWindow
    {
        CircularBuffer<ColoredString> Messages;

        private bool NeedsRelayout;

        private FontSpec FontTargetText = UiFonts.TargetText;
        private IUiText[] TextMessages;

        public SimpleMessageWindow()
        {
            Messages = new CircularBuffer<ColoredString>(50);
            TextMessages = new IUiText[10];
            for (int i = 0; i < TextMessages.Length; i++)
            {
                TextMessages[i] = new UiTextOutlined(FontTargetText);
            }
            NeedsRelayout = true;
        }

        public void Print(string text, Color? color = null)
        {
            if (color == null)
                color = Color.White;

            Messages.PushFront(new ColoredString(text, color.Value));
            NeedsRelayout = true;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            NeedsRelayout = true;
        }

        private void RelayoutText()
        {
            for (int i = 0; i < TextMessages.Length; i++)
            {
                if (i >= Messages.Size)
                {
                    break;
                }

                var uiText = TextMessages[i];
                var line = Messages[i];
                uiText.Text = line.Text;
                uiText.Color = line.Color;
            }

            NeedsRelayout = false;
        }

        public override void Update(float dt)
        {
            if (NeedsRelayout)
            {
                RelayoutText();
            }

            for (int i = 0; i < TextMessages.Length; i++)
            {
                TextMessages[i].Update(dt);
            }
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(0, 0, 0, 64);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, X, Y, Width, Height);

            //Love.Graphics.SetScissor(this.X, this.Y, this.Width, this.Height);
            for (int i = 0; i < TextMessages.Length; i++)
            {
                var text = TextMessages[i];
                text.SetPosition(X + 5, Y + Height - FontTargetText.LoveFont.GetHeight() * (i + 1) - 5);
                text.Draw();
            }
            //Love.Graphics.SetScissor();
        }

        public override void Dispose()
        {
            for (int i = 0; i < TextMessages.Length; i++)
            {
                TextMessages[i].Dispose();
            }
        }
    }
}