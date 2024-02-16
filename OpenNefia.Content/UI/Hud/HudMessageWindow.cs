﻿using Microsoft.Extensions.ObjectPool;
using OpenNefia.Content.Hud;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
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
        public static FormattedMessage WithColor(string text, Color? color = null)
        {
            var mes = new FormattedMessage();
            mes.AddTag(new ColorMessageTag(color));
            mes.AddTag(new TextMessageTag(text));
            return mes;
        }
    }

    public class HudMessageWindow : BaseHudWidget, IHudMessageWindow
    {
        [Dependency] private readonly IConfigurationManager _config = default!;
        public class MessageContainer : UiHorizontalContainer
        {
            public bool HasContent => _entries.Any(x => x.Element is MessageText);
            public void SetOpacities(byte opacity)
            {
                foreach(var item in _entries)
                {
                    if (item.Element is MessageText text && text.ChangeOpacity)
                        text.Color = text.Color.WithAlphaB(opacity);
                }
            }
        }

        public class MessageText : UiText
        {
            public bool ChangeOpacity { get; set; }

            public MessageText() : base(UiFonts.MessageText, string.Empty)
            {
            }

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
        private const int MaxMessages = 500;

        private float MessageFadeAmount => (float)_config.GetCVar(CCVars.MessageFade) / 100f;
        private bool NeedsRelayout;

        public HudMessageWindow(UiContainer messageBoxContainer, UiContainer backLogContainer)
        {
            IoCManager.InjectDependencies(this);
            //not the amount of lines, but the amount of messages
            Messages = new CircularBuffer<FormattedMessage>(MaxMessages);
            MessageBoxContainer = messageBoxContainer;
            BacklogContainer = backLogContainer;
        }

        public void Print(string queryText, Color? color = null)
        {
            var mes = FormattedMessage.WithColor(queryText, color);
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

        public void Clear()
        {
            Messages.Clear();
            NeedsRelayout = true;
        }

        private MessageContainer GetMessageLine(bool newLine)
        {
            var cont = new MessageContainer();
            if (newLine)
                cont.AddLayout(LayoutType.Spacer, 15);
            return cont;
        }

        private ObjectPool<MessageText> _messagePool = new DefaultObjectPool<MessageText>(new DefaultPooledObjectPolicy<MessageText>());

        private void RelayoutText()
        {
            var lines = new List<MessageContainer>();
            var currentLine = GetMessageLine(false);
            Color currentColor = Color.White;
            var totalWidth = 0f;

            foreach (var container in MessageBoxContainer.Children.Cast<MessageContainer>())
            {
                foreach (var entry in container.Entries)
                {
                    if (entry.Element is MessageText messageText)
                    {
                        _messagePool.Return(messageText);
                    }
                }
            }
            foreach (var container in BacklogContainer.Children.Cast<MessageContainer>())
            {
                foreach (var entry in container.Entries)
                {
                    if (entry.Element is MessageText messageText)
                    {
                        _messagePool.Return(messageText);
                    }
                }
            }

            MessageBoxContainer.Clear();
            BacklogContainer.Clear();

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
                            totalWidth = 0f;
                            break;

                        case ColorMessageTag colorTag:
                            currentColor = colorTag.Color;
                            break;

                        case TextMessageTag textTag:
                            var sb = new StringBuilder();
                            var words = UiHelpers.SplitString(textTag.Message, Loc.Language);
                            foreach(var word in words)
                            {
                                var wordWidth = UiFonts.MessageText.LoveFont.GetWidthV(UIScale, word);
                                if (totalWidth + wordWidth > Width)
                                {
                                    var text = _messagePool.Get();
                                    text.Text = sb.ToString().TrimStart();
                                    text.Color = currentColor;
                                    currentLine.AddElement(text);
                                    lines.Add(currentLine);
                                    currentLine = GetMessageLine(false);
                                    totalWidth = 0;
                                    sb.Clear();
                                }
                                sb.Append(word);
                                totalWidth += wordWidth;
                            }
                            var text2 = _messagePool.Get();
                            text2.Text = $"{sb.ToString().TrimStart()}{Loc.Space}";
                            text2.Color = currentColor;
                            currentLine.AddElement(text2);
                            break;

                        case IconMessageTag icon:
                            //TODO display icons
                            break;
                    }
                }
            }

            lines.Add(currentLine);

            var emptyLine = new MessageContainer();
            emptyLine.AddElement(new UiText(UiFonts.MessageText));

            var lineIndex = 0;

            var mesBoxLines = Enumerable.Repeat(emptyLine, Math.Max(0, MessageBoxLines - lines.Count))
                .Concat(lines.Skip(Math.Max(0, lines.Count - MessageBoxLines)));

            var backlogLines = lines.Count > MessageBoxLines
                ? Enumerable.Repeat(emptyLine, Math.Max(0, BacklogLines + MessageBoxLines - lines.Count))
                    .Concat(lines.Take(lines.Count - MessageBoxLines))
                : Enumerable.Empty<MessageContainer>();

            foreach (var line in mesBoxLines)
            {
                line.SetOpacities(Convert.ToByte(255 * Math.Pow(MessageFadeAmount, 3 - lineIndex)));
                MessageBoxContainer.AddElement(line);
                lineIndex++;
            }

            foreach (var line in backlogLines)
            {
                line.SetOpacities(Convert.ToByte(255));
                if (lineIndex >= MessageBoxLines + BacklogLines)
                    break;
                BacklogContainer.AddElement(line);
                lineIndex++;
            }

            MessageBoxContainer.Relayout();
            BacklogContainer.Relayout();
            NeedsRelayout = false;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            NeedsRelayout = true;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (NeedsRelayout)
                RelayoutText();
        }
    }
}
