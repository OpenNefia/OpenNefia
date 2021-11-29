using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.UI.Hud
{
    public class ColoredString
    {
        public readonly string Text;
        public readonly Color Color;

        public ColoredString(string text, Color color)
        {
            this.Text = text;
            this.Color = color;
        }
    }

    public class SimpleMessageWindow : BaseUiElement, IHudMessageWindow
    {
        CircularBuffer<ColoredString> Messages;

        private bool NeedsRelayout;

        [UiStyled] private FontSpec FontTargetText = new();
        private IUiText[] TextMessages;

        public SimpleMessageWindow()
        {
            Messages = new CircularBuffer<ColoredString>(50);
            TextMessages = new IUiText[6];
            for (int i = 0; i < TextMessages.Length; i++)
            {
                TextMessages[i] = new UiText(FontTargetText);
            }
            NeedsRelayout = true;
        }

        public void Print(string text, Color? color = null)
        {
            if (color == null)
                color = Color.White;

            this.Messages.PushFront(new ColoredString(text, color.Value));
            this.NeedsRelayout = true;
        }

        public override void SetPosition(Vector2i pos)
        {
            base.SetPosition(pos);
            this.NeedsRelayout = true;
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

            this.NeedsRelayout = false;
        }

        public override void Update(float dt)
        {
            if (this.NeedsRelayout)
            {
                this.RelayoutText();
            }

            for (int i = 0; i < TextMessages.Length; i++)
            {
                TextMessages[i].Update(dt);
            }
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(0, 0, 0, 64);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.Left, this.Top, this.Width, this.Height);

            //GraphicsEx.SetScissor(this.X, this.Y, this.Width, this.Height);
            for (int i = 0; i < TextMessages.Length; i++)
            {
                var text = TextMessages[i];
                text.SetPosition(Left + 5, Top + Height - (FontTargetText.LoveFont.GetHeight()) * (i + 1) - 5);
                text.Draw();
            }
            //GraphicsEx.SetScissor();
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