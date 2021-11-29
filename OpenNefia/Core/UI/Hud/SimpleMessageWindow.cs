using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;

namespace OpenNefia.Core.UI.Hud
{
    public class SimpleMessageWindow : BaseUiElement, IHudMessageWindow
    {
        CircularBuffer<ColoredString> Messages;

        private bool NeedsRelayout;

        private FontSpec FontTargetText;
        private IUiText[] TextMessages;

        public SimpleMessageWindow()
        {
            FontTargetText = FontDefOf.TargetText;
            Messages = new CircularBuffer<ColoredString>(50);
            TextMessages = new IUiText[6];
            for (int i = 0; i < TextMessages.Length; i++)
            {
                TextMessages[i] = new UiText(FontTargetText);
            }
            NeedsRelayout = true;
        }

        public void Print(string text, Love.Color? color = null)
        {
            if (color == null)
                color = Love.Color.White;

            this.Messages.PushFront(new ColoredString(text, color.Value));
            this.NeedsRelayout = true;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
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
                uiText.Text = line.text;
                uiText.Color = new Love.Color(line.color.r, line.color.g, line.color.b, line.color.a);
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
                text.SetPosition(Left + 5, Top + Height - (FontTargetText.GetHeight()) * (i + 1) - 5);
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