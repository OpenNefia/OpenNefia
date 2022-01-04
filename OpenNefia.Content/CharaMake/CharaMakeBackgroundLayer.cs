using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    /// <summary>
    /// Made this before realizing backgrounds were elona+ only...
    /// </summary>
    public class CharaMakeBackgroundLayer : CharaMakeLayer
    {
        public enum PastChoice
        {
            Proceed,
            KeepSecret,
            RerollAll,
            RerollFirst,
            RerollSecond
        }

        public class PastCell : UiListCell<PastChoice>
        {
            public override string? LocalizeKey => Enum.GetName(Data);

            public PastCell(PastChoice data) 
                : base(data, new UiText(UiFonts.ListTitleScreenText))
            {
            }
        }

        private UiText PastText;
        private string[] PastParts;

        [Localize]
        private UiWindow Window;
        [Localize]
        private IUiText PastTopic;
        [Localize]
        private UiList<PastChoice> List;

        public CharaMakeBackgroundLayer()
        {
            Window = new UiWindow();
            PastTopic = new UiTextTopic();

            PastParts = new string[5];
            PastText = new UiText();
            RerollPast();

            List = new UiList<PastChoice>(Enum.GetValues<PastChoice>().Select(x => new PastCell(x)));
            List.EventOnActivate += (_, args) =>
            {
                switch(args.SelectedCell.Data)
                {
                    case PastChoice.Proceed:
                        Finish(new CharaMakeResult(new Dictionary<string, object>
                        {
                            { "past", string.Join(Environment.NewLine, PastParts) }
                        }));
                        break;
                    case PastChoice.KeepSecret:
                        Finish(new CharaMakeResult(new Dictionary<string, object>
                        {
                            { "past", Loc.GetString("PastSecret") }
                        }));
                        break;
                    default:
                        RerollPast(args.SelectedCell.Data);
                        break;
                }
            };

            AddChild(Window);
            AddChild(List);
            AddChild(PastText);
        }

        private void RerollPast(PastChoice choice = PastChoice.RerollAll)
        {
            int start = 0, end = 5;
            switch (choice)
            {
                case PastChoice.RerollFirst:
                    end = 2;
                    break;
                case PastChoice.RerollSecond:
                    start = 2;
                    break;
            }
            for (int i = start; i < end; i++)
            {
                //TODO add background flavour texts + localization
                PastParts[i] = GetPast(i);
            }
            PastText.Text = string.Join(Environment.NewLine, PastParts);
        }

        //probably want to get some way to get a key to a text instead so the past is localized properly when 
        //      changing langauges
        private string GetPast(int index)
        {
            return Guid.NewGuid().ToString() + (index == 2 ? "," : ".");
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(400, 350);
            PastTopic.SetPreferredSize();
            List.SetPreferredSize();
            PastText.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window);
            PastTopic.SetPosition(Window.X + 25, Window.Y + 30);
            List.SetPosition(Window.X + 40, Window.Y + 68);
            PastText.SetPosition(Window.X + 80, Window.Y + PastTopic.Height + List.Height + 80);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            PastTopic.Draw();
            List.Draw();
            PastText.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            PastTopic.Update(dt);
            List.Update(dt);
            PastText.Update(dt);
        }
    }
}
