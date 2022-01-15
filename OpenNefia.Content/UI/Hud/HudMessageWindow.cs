using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Hud
{
    public class MessageTag
    {

    }

    public class ColorMessageTag : MessageTag
    {
        private Color Color;

        public ColorMessageTag(Color? color)
        {
            Color = color ?? UiColors.MesWhite;
        }
    }

    public class TextMessageTag : MessageTag
    {
        private string Message;

        public TextMessageTag(string message)
        {
            Message = message;
        }
    }

    public class NewLineMessageTag : MessageTag
    {

    }

    public class IconMessageTag : MessageTag
    {

    }

    public class FormattedMessage
    {
        private List<MessageTag> Tags;
        public FormattedMessage(IEnumerable<MessageTag>? tags = null)
        {
            Tags = tags?.ToList() ?? new();
        }
        public void Add(MessageTag tag)
        {
            Tags.Add(tag);
        }
        public static FormattedMessage ColoredMessage(string text, Color? color = null, bool newLine = true)
        {
            var mes = new FormattedMessage();
            if (newLine)
                mes.Add(new NewLineMessageTag());
            mes.Add(new ColorMessageTag(color));
            mes.Add(new TextMessageTag(text));
            return mes;
        }
    }

    public class HudMessageWindow : UiElement, IHudMessageWindow
    {
        private CircularBuffer<FormattedMessage> Messages;
        private UiContainer BackLogContainer;
        private UiContainer MessageBoxContainer;

        private const int MaxMessageLines = 29;

        private bool NeedsRelayout;
        public HudMessageWindow(UiContainer messageBoxContainer, UiContainer backLogContainer)
        {
            Messages = new CircularBuffer<FormattedMessage>(40);
            MessageBoxContainer = messageBoxContainer;
            BackLogContainer = backLogContainer;
        }

        public void Print(string queryText, Color? color = null, bool newLine = true)
        {
            Messages.PushFront(FormattedMessage.ColoredMessage(queryText, color, newLine));
            NeedsRelayout = true;
        }

        private void ReLayoutText()
        {
            var font = UiFonts.MessageText;
            foreach (var message in Messages)
            {
                
            }
            NeedsRelayout = false;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            NeedsRelayout = true;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (NeedsRelayout)
                ReLayoutText();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}
