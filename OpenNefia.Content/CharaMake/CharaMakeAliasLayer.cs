using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeAliasLayer : CharaMakeLayer
    {
        public class CreateCharAlias
        {
            public CreateCharAlias(string alias, bool isReroll = false)
            {
                Alias = alias;
                IsReroll = isReroll;
            }
            public string Alias { get; set; }
            public bool IsReroll { get; set; }
        }
        public class CreateCharAliasCell : UiListCell<CreateCharAlias>
        {
            public CreateCharAliasCell(CreateCharAlias data, string text)
                : base(data, new UiText(UiFonts.ListTitleScreenText, text))
            {
            }
        }

        [Localize]
        private UiWindow Window;
        [Localize]
        private IUiText AliasTopic;

        private UiList<CreateCharAlias> AliasList;

        public CharaMakeAliasLayer()
        {
            Window = new UiWindow();
            var items = new CreateCharAliasCell[17];
            items[0] = new CreateCharAliasCell(new CreateCharAlias(string.Empty, true), Loc.GetString("AliasReroll"));

            for (int i = 1; i < items.Length; i++)
            {
                //TODO add alias generation + localization support
                var alias = Guid.NewGuid().ToString();
                items[i] = new CreateCharAliasCell(new CreateCharAlias(alias, false), alias);
            }

            AliasList = new UiList<CreateCharAlias>(items);
            AliasList.EventOnActivate += (_, args) =>
            {
                if (args.SelectedCell.Data.IsReroll)
                {
                    Sounds.Play(Prototypes.Protos.Sound.Dice);
                    //TODO add rerolling
                }
                else
                {
                    Result = new CharaMakeResult(new Dictionary<string, object>
                    {
                        { "alias", args.SelectedCell.Data.Alias }
                    });
                    Finish(Result);
                }
            };
            AliasTopic = new UiTextTopic();

            AddChild(Window);
            AddChild(AliasList);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(400, 470);
            AliasTopic.SetPreferredSize();
            AliasList.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window);
            AliasTopic.SetPosition(Window.X + 25, Window.Y + 30);
            AliasList.SetPosition(Window.X + 40, Window.Y + 68);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            AliasList.Draw();
            AliasTopic.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            AliasTopic.Update(dt);
            AliasList.Update(dt);
        }
    }
}
