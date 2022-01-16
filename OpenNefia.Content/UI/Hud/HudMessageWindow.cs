using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Hud
{
    public abstract class MessageTag
    {
    }

    public class ColorMessageTag : MessageTag
    {
        public Color Color;

        public ColorMessageTag(Color? color)
        {
            Color = color ?? UiColors.MesWhite;
        }
    }

    public class TextMessageTag : MessageTag
    {
        public string Message;

        public TextMessageTag(string message)
        {
            Message = message;
        }
    }

    public class NewlineMessageTag : MessageTag
    {

    }

    public class IconMessageTag : MessageTag
    {

    }

    public class FormattedMessage
    {
        public List<MessageTag> Tags;
        public FormattedMessage(IEnumerable<MessageTag>? tags = null)
        {
            Tags = tags?.ToList() ?? new();
        }
        public void AddTag(MessageTag tag)
        {
            Tags.Add(tag);
        }
        public static FormattedMessage ColoredMessage(string text, Color? color = null)
        {
            var mes = new FormattedMessage();
            mes.AddTag(new ColorMessageTag(color));
            mes.AddTag(new TextMessageTag(text));
            return mes;
        }
    }

    public class HudMessageWindow : UiElement, IHudMessageWindow
    {
        public class MessageContainer : UiHorizontalContainer
        {
            public bool HasContent => Entries.Any(x => x.Element is MessageText);
            public void SetOpacities(byte opacity)
            {
                foreach(var item in Entries)
                {
                    if (item.Element is MessageText text && text.ChangeOpacity)
                        text.Color = text.Color.WithAlpha(opacity);
                }
            }
        }

        public class MessageText : UiText
        {
            public bool ChangeOpacity { get; set; }

            public MessageText(string text, bool changeOpacity = true) : base(UiFonts.MessageText, text)
            {
                ChangeOpacity = changeOpacity;
            }
        }

        private CircularBuffer<FormattedMessage> Messages;
        private UiContainer BacklogContainer;
        private UiContainer MessageBoxContainer;

        private const int BacklogLines = 26;
        private const int MessageBoxLines = 4;

        private readonly byte[] MessageOpacities =
        {
            130,
            180,
            220,
            255,
            210
        };

        private bool NeedsRelayout;
        public HudMessageWindow(UiContainer messageBoxContainer, UiContainer backLogContainer)
        {
            Messages = new CircularBuffer<FormattedMessage>(200);
            MessageBoxContainer = messageBoxContainer;
            BacklogContainer = backLogContainer;
        }

        public void Print(string queryText, Color? color = null)
        {
            var mes = FormattedMessage.ColoredMessage(queryText, color);
            if (Messages.IsEmpty)
                mes.Tags.Insert(0, new NewlineMessageTag());
            Messages.PushFront(mes);
            NeedsRelayout = true;
        }

        public void Newline()
        {
            var mes = new FormattedMessage();
            mes.AddTag(new NewlineMessageTag());
            Messages.PushFront(mes);
            NeedsRelayout = true;
        }

        private MessageContainer GetMessageLine(bool newLine)
        {
            var cont = new MessageContainer();
            if (newLine)
                cont.AddLayout(LayoutType.Spacer, 15);
            return cont;
        }

        private void ReLayoutText()
        {
            var lines = new List<MessageContainer>();
            var currentLine = GetMessageLine(false);
            Color currentColor = Color.White;
            var totalWidth = 0;
            foreach (var message in Messages.Reverse())
            {
                foreach(var tag in message.Tags)
                {
                    switch (tag)
                    {
                        case NewlineMessageTag newLine:
                            if (currentLine.HasContent)
                                lines.Add(currentLine);
                            currentLine = GetMessageLine(true);
                            totalWidth = 0;
                            break;

                        case ColorMessageTag colorTag:
                            currentColor = colorTag.Color;
                            break;

                        case TextMessageTag textTag:
                            var sb = new StringBuilder();
                            var words = UiHelpers.SplitString(textTag.Message, Loc.Language);
                            foreach(var word in words)
                            {
                                var wordWidth = UiFonts.MessageText.LoveFont.GetWidth(word);
                                if (totalWidth + wordWidth > Width)
                                {
                                    currentLine.AddElement(new MessageText(sb.ToString().TrimStart()) { Color = currentColor });
                                    lines.Add(currentLine);
                                    currentLine = GetMessageLine(false);
                                    totalWidth = 0;
                                    sb.Clear();
                                }
                                sb.Append(word);
                                totalWidth += wordWidth;
                            }
                            currentLine.AddElement(new MessageText($"{sb.ToString().TrimStart()} ") { Color = currentColor });
                            break;

                        case IconMessageTag icon:
                            //TODO display icons
                            break;
                    }
                }
            }
            lines.Add(currentLine);
            MessageBoxContainer.Clear();
            BacklogContainer.Clear();

            var emptyLine = new MessageContainer();
            emptyLine.AddElement(new UiText(UiFonts.MessageText));

            var lineIndex = 0;
            var mesBoxLines = Enumerable.Empty<MessageContainer>()
                .Concat(Enumerable.Repeat(emptyLine, Math.Max(0, MessageBoxLines - lines.Count)))
                .Concat(lines.Skip(Math.Max(0, lines.Count - MessageBoxLines))); 
            var backlogLines = lines.Count > MessageBoxLines
                ? Enumerable.Empty<MessageContainer>()
                    .Concat(Enumerable.Repeat(emptyLine, Math.Max(0, BacklogLines + MessageBoxLines - lines.Count)))
                    .Concat(lines.Take(lines.Count - MessageBoxLines))
                : Enumerable.Empty<MessageContainer>();

            foreach (var line in mesBoxLines)
            {
                line.SetOpacities(MessageOpacities[lineIndex]);
                MessageBoxContainer.AddElement(line);
                lineIndex++;
            }
            foreach (var line in backlogLines)
            {
                line.SetOpacities(MessageOpacities[4]);
                if (lineIndex >= MessageBoxLines + BacklogLines)
                    break;
                BacklogContainer.AddElement(line);
                lineIndex++;
            }
            MessageBoxContainer.Resolve();
            BacklogContainer.Resolve();
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
    }
}
