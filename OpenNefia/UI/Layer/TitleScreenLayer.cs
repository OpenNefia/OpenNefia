using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Element.List;

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

            [Localize("Subtext")]
            public IUiText TextSubtext;

            public override string? LocalizeKey => Enum.GetName(Submenu);

            public TitleScreenCell(TitleScreenChoice submenu)
                : base(submenu, new UiText())
            {
                this.Submenu = submenu;
                this.TextSubtext = new UiText();
            }

            public override void Localize(LocaleKey key)
            {
                base.Localize(key);
                this.SetPosition(X, Y);
            }

            public override void SetPosition(int x, int y)
            {
                base.SetPosition(x, y);
                if (this.TextSubtext.Text != string.Empty)
                {
                    this.TextSubtext.SetPosition(y + 40, y - 4);
                    this.UiText.SetPosition(x + 40 + this.XOffset + 4, y + 8);
                }
                else
                {
                    this.UiText.SetPosition(x + 40 + this.XOffset + 4, y + 1);
                }
            }

            public override void SetSize(int width, int height)
            {
                height = ITEM_HEIGHT;

                this.UiText.SetSize(width, height);
                this.TextSubtext.SetSize(width, height);
                base.SetSize(Math.Max(width, this.UiText.Width), height);
            }

            public override void Draw()
            {
                GraphicsEx.SetColor(Love.Color.White);
                this.AssetSelectKey.Draw(this.X, this.Y - 1);
                this.KeyNameText.Draw();
                this.UiText.Draw();
                if (Loc.IsFullwidth())
                {
                    this.TextSubtext.Draw();
                }
            }

            public override void Update(float dt)
            {
                this.KeyNameText.Update(dt);
                this.UiText.Update(dt);
                this.TextSubtext.Update(dt);
            }

            public override void Dispose()
            {
                base.Dispose();
                this.TextSubtext.Dispose();
            }
        }

        private FontSpec FontTitleText = UiFonts.TitleScreenText;
        private IAssetDrawable AssetTitle;
        private IAssetDrawable AssetG4;

        private IUiText[] TextInfo;
        
        [Localize]
        private UiWindow Window;

        [Localize]
        private UiList<TitleScreenChoice> List;

        public TitleScreenLayer()
        {
            AssetTitle = Assets.Get(AssetPrototypeOf.Title);
            AssetG4 = Assets.Get(AssetPrototypeOf.G4);

            var version = "1.22";
            TextInfo = new IUiText[3];

            TextInfo[0] = new UiText(FontTitleText, $"Elona version {version}  Developed by Noa");
            if (Loc.Language == LanguagePrototypeOf.Japanese)
            {
                TextInfo[1] = new UiText(FontTitleText, "Contributor MSL / View the credits for more");
            }
            else if (Loc.Language == LanguagePrototypeOf.English)
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
            TextInfo[0].SetPosition(this.X + 20, this.Y + 20);
            TextInfo[1].SetPosition(this.X + 20, this.Y + 20 + (FontTitleText.LoveFont.GetHeight() + 5));
            TextInfo[2].SetPosition(this.X + 20, this.Y + 20 + (FontTitleText.LoveFont.GetHeight() + 5) * 2);
            this.Window.SetPosition(this.X + 80, (this.Height - 308) / 2);
            this.List.SetPosition(this.Window.X + 40, this.Window.Y + 48);
        }

        public override void OnQuery()
        {
            Music.Play(MusicPrototypeOf.Opening);
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
            this.AssetTitle.Draw(this.X, this.Y, this.Width, this.Height);

            foreach (var text in this.TextInfo)
                text.Draw();

            this.Window.Draw();
            this.List.Draw();

            var bgPicWidth = this.Window.Width / 5 * 4;
            var bgPicHeight = this.Window.Height - 80;
            GraphicsEx.SetColor(255, 255, 255, 50);
            this.AssetG4.Draw(this.Window.X + 160 - (bgPicWidth / 2),
                              this.Window.Y + this.Window.Height / 2 - (bgPicHeight / 2),
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
        }
    }
}
