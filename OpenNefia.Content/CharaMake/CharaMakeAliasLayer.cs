using OpenNefia.Content.RandomText;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.AliasSelect")]
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

        [Dependency] private readonly IRandomAliasGenerator _aliasGenerator = default!;

        public const string ResultName = "alias";

        [Localize]
        private UiWindow Window;
        [Localize]
        private IUiText AliasTopic;

        private UiList<CreateCharAlias> List;

        public CharaMakeAliasLayer()
        {
            Window = new UiWindow();

            List = new UiList<CreateCharAlias>();
            List.EventOnActivate += HandleListOnActivate;
            AliasTopic = new UiTextTopic();

            AddChild(Window);
            AddChild(List);
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            Reroll();
        }

        private void HandleListOnActivate(object? sender, UiListEventArgs<CreateCharAlias> args)
        {
            if (args.SelectedCell.Data.IsReroll)
            {
                Sounds.Play(Sound.Dice);
                Reroll();
            }
            else
            {
                Result = new CharaMakeResult(new Dictionary<string, object>
                {
                    { ResultName, args.SelectedCell.Data.Alias }
                });
                Finish(Result);
            }
        }

        private void Reroll()
        {
            var items = new CreateCharAliasCell[17];
            items[0] = new CreateCharAliasCell(new CreateCharAlias(string.Empty, true), Loc.GetString("Elona.CharaMake.AliasSelect.Reroll"));

            for (int i = 1; i < items.Length; i++)
            {
                var alias = _aliasGenerator.GenerateRandomAlias(AliasType.Chara);
                items[i] = new CreateCharAliasCell(new CreateCharAlias(alias, false), alias);
            }

            List.Clear();
            List.AddRange(items);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(400, 470);
            AliasTopic.SetPreferredSize();
            List.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window);
            AliasTopic.SetPosition(Window.X + 25, Window.Y + 30);
            List.SetPosition(Window.X + 40, Window.Y + 68);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            GraphicsEx.SetColor(255, 255, 255, 30);
            CurrentWindowBG.Draw(Window.X + 40, Window.Y + 30, 300, 405);
            List.Draw();
            AliasTopic.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            AliasTopic.Update(dt);
            List.Update(dt);
        }
    }
}
