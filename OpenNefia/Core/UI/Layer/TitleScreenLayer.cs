using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Element.List;
using OpenNefia.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class TitleScreenLayer : BaseUiLayer<TitleScreenResult>, ITitleScreenLayer
    {
        private enum TitleScreenChoice
        {
            Restore,
            Generate,
            Incarnate,
            About,
            Options,
            Mods,
            Exit
        }

        private class TitleScreenCell : UiListCell<TitleScreenChoice>
        {
            private const int ITEM_HEIGHT = 35;
            
            public TitleScreenChoice Submenu;

            [Localize(Key="Subtext")]
            public IUiText UiTextSubtext;

            public override string? LocalizeKey => Enum.GetName(Submenu);

            public TitleScreenCell(TitleScreenChoice submenu)
                : base(submenu, new UiText(FontDefOf.ListTitleScreenText))
            {
                this.Submenu = submenu;
                this.UiTextSubtext = new UiText(FontDefOf.ListTitleScreenSubtext);
            }

            public override void Localize(LocaleKey key)
            {
                base.Localize(key);
                this.SetPosition(Left, Top);
            }

            public override void SetPosition(int x, int y)
            {
                base.SetPosition(x, y);
                if (this.UiTextSubtext.Text != string.Empty)
                {
                    this.UiTextSubtext.SetPosition(x + 40, y - 4);
                    this.UiText.SetPosition(x + 40 + this.XOffset + 4, y + 8);
                }
                else
                {
                    this.UiText.SetPosition(x + 40 + this.XOffset + 4, y + 1);
                }
            }

            public override void SetSize(int width = -1, int height = -1)
            {
                height = ITEM_HEIGHT;

                this.UiText.SetSize(width, height);
                this.UiTextSubtext.SetSize(width, height);
                base.SetSize(Math.Max(width, this.UiText.Width), height);
            }

            public override void Draw()
            {
                GraphicsEx.SetColor(Love.Color.White);
                this.AssetSelectKey.Draw(this.Left, this.Top - 1);
                this.KeyNameText.Draw();
                this.UiText.Draw();
                if (I18N.IsFullwidth())
                {
                    this.UiTextSubtext.Draw();
                }
            }

            public override void Update(float dt)
            {
                this.KeyNameText.Update(dt);
                this.UiText.Update(dt);
                this.UiTextSubtext.Update(dt);
            }

            public override void Dispose()
            {
                base.Dispose();
                this.UiTextSubtext.Dispose();
            }
        }

        private FontSpec FontTitleText;
        private AssetDrawable AssetTitle;
        private AssetDrawable AssetG4;

        private IUiText[] TextInfo;
        
        [Localize]
        private UiWindow Window;

        [Localize]
        private UiList<TitleScreenChoice> List;

        public TitleScreenLayer()
        {
            FontTitleText = FontDefOf.TitleScreenText;
            AssetTitle = new AssetDrawable(AssetPrototypeOf.Title);
            AssetG4 = new AssetDrawable(AssetPrototypeOf.G4);

            var version = "1.22";
            TextInfo = new IUiText[3];

            TextInfo[0] = new UiText(FontTitleText, $"Elona version {version}  Developed by Noa");
            if (I18N.Language == "ja_JP")
            {
                TextInfo[1] = new UiText(FontTitleText, "Contributor MSL / View the credits for more");
            }
            else if (I18N.Language == "en_US")
            {
                TextInfo[1] = new UiText(FontTitleText, "Contributor f1r3fly, Sunstrike, Schmidt, Elvenspirit / View the credits for more");
            }
            TextInfo[2] = new UiText(FontTitleText, $"{Engine.NameBase} version {Engine.Version} Developed by Ruin0x11");

            Window = new UiWindow();

            var items = new List<TitleScreenCell>() {
                new TitleScreenCell(TitleScreenChoice.Restore),
                new TitleScreenCell(TitleScreenChoice.Generate),
                new TitleScreenCell(TitleScreenChoice.Incarnate),
                new TitleScreenCell(TitleScreenChoice.About),
                new TitleScreenCell(TitleScreenChoice.Options),
                new TitleScreenCell(TitleScreenChoice.Mods),
                new TitleScreenCell(TitleScreenChoice.Exit),
            };
            List = new UiList<TitleScreenChoice>(items);
            List.EventOnActivate += (_, evt) => this.RunTitleScreenAction(evt.SelectedCell.Data);
            
            this.Forwards += List;
        }

        private void RunTitleScreenAction(TitleScreenChoice selectedChoice)
        {
            if (selectedChoice != TitleScreenChoice.Generate)
            {
                Sounds.Play(SoundPrototypeOf.Ok1);
            }

            switch (selectedChoice)
            {
                case TitleScreenChoice.Restore:
                    this.Finish(new TitleScreenResult(TitleScreenAction.StartGame));
                    break;
                case TitleScreenChoice.Exit:
                    this.Finish(new TitleScreenResult(TitleScreenAction.Quit));
                    break;
                default:
                    break;
            }
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            this.Window.SetSize(320, 355);
            this.List.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            TextInfo[0].SetPosition(this.Left + 20, this.Top + 20);
            TextInfo[1].SetPosition(this.Left + 20, this.Top + 20 + (FontTitleText.GetHeight() + 5));
            TextInfo[2].SetPosition(this.Left + 20, this.Top + 20 + (FontTitleText.GetHeight() + 5) * 2);
            this.Window.SetPosition(this.Left + 80, (this.Height - 308) / 2);
            this.List.SetPosition(this.Window.Left + 40, this.Window.Top + 48);
        }

        public override void OnQuery()
        {
            Music.PlayMusic(MusicDefOf.Opening);
        }

        public override void Update(float dt)
        {
            foreach (var text in this.TextInfo)
                text.Update(dt);

            this.Window.Update(dt);
            this.List.Update(dt);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Love.Color.White);
            this.AssetTitle.Draw(this.Left, this.Top, this.Width, this.Height);

            foreach (var text in this.TextInfo)
                text.Draw();

            this.Window.Draw();
            this.List.Draw();

            var bgPicWidth = this.Window.Width / 5 * 4;
            var bgPicHeight = this.Window.Height - 80;
            GraphicsEx.SetColor(255, 255, 255, 50);
            this.AssetG4.Draw(this.Window.Left + 160 - (bgPicWidth / 2),
                              this.Window.Top + this.Window.Height / 2 - (bgPicHeight / 2),
                              bgPicWidth,
                              bgPicHeight);
        }

        public override void Dispose()
        {
            foreach (var text in TextInfo)
            {
                text.Dispose();
            }
            Window.Dispose();
            List.Dispose();
            AssetTitle.Dispose();
            AssetG4.Dispose();
        }
    }
}
