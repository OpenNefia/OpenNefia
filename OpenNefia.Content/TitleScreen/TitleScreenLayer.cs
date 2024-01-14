using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Input;

namespace OpenNefia.Content.TitleScreen
{
    [Localize("Elona.TitleScreen.Layer")]
    public class TitleScreenLayer : UiLayerWithResult<UINone, TitleScreenResult>, ITitleScreenLayer
    {
        [Dependency] private readonly IAudioManager _audio = default!;

        public enum TitleScreenChoice
        {
            Restore,
            Generate,
            Incarnate,
            About,
            Options,
            Mods,
            Exit
        }

        public class TitleScreenCell : UiListCell<TitleScreenChoice>
        {
            private const float ITEM_HEIGHT = 35;

            [Child] [Localize("Subtext")] public UiText TextSubtext;

            public override string? LocalizeKey => Enum.GetName(Data);

            public TitleScreenCell(TitleScreenChoice submenu)
                : base(submenu, new UiText(UiFonts.ListTitleScreenText))
            {
                TextSubtext = new UiText(UiFonts.ListTitleScreenSubtext);
            }

            public override void Localize(LocaleKey key)
            {
                base.Localize(key);
                SetPosition(X, Y);
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                if (TextSubtext.Text != string.Empty)
                {
                    TextSubtext.SetPosition(X + 40, Y - 5);
                    UiText.SetPosition(X + 40 + XOffset + 4, Y + 7);
                }
                else
                {
                    UiText.SetPosition(X + 40 + XOffset + 4, Y);
                }
            }

            public override void SetSize(float width, float height)
            {
                height = ITEM_HEIGHT;

                UiText.SetSize(width, height);
                TextSubtext.SetSize(width, height);
                base.SetSize(Math.Max(width, UiText.Width), height);
            }

            public override void Draw()
            {
                GraphicsEx.SetColor(Love.Color.White);
                AssetSelectKey.Draw(UIScale, X, Y - 1);
                KeyNameText.Draw();
                UiText.Draw();
                if (Loc.IsFullwidth())
                {
                    TextSubtext.Draw();
                }
            }

            public override void Update(float dt)
            {
                KeyNameText.Update(dt);
                UiText.Update(dt);
                TextSubtext.Update(dt);
            }

            public override void Dispose()
            {
                base.Dispose();
                TextSubtext.Dispose();
            }
        }

        private FontSpec FontTitleText = UiFonts.TitleScreenText;
        private IAssetInstance AssetG4;

        private UiText[] TextInfo;
        [Child] private UiText TextQuickStart;

        [Child] [Localize] private UiWindow Window;
        [Child] [Localize] private UiList<TitleScreenChoice> List;

        public TitleScreenLayer()
        {
            AssetG4 = Assets.Get(Protos.Asset.G4);

            var version = "1.22";
            TextInfo = new UiText[3];

            TextInfo[0] = new UiText(FontTitleText, $"Elona version {version}  Developed by Noa");
            if (Loc.Language == LanguagePrototypeOf.Japanese)
            {
                TextInfo[1] = new UiText(FontTitleText, "Contributor MSL / View the credits for more");
            }
            else // if (Loc.Language == LanguagePrototypeOf.English)
            {
                TextInfo[1] = new UiText(FontTitleText, "Contributor f1r3fly, Sunstrike, Schmidt, Elvenspirit / View the credits for more");
            }
            TextInfo[2] = new UiText(FontTitleText, $"{Engine.NameBase} version {Engine.Version} Developed by Ruin0x11");

            foreach (var text in TextInfo)
            {
                AddChild(text);
            }

            var quickStartKey = IoCManager.Resolve<IInputManager>()
                .GetKeyFunctionButtonString(ContentKeyFunctions.QuickStart);
            TextQuickStart = new UiText(FontTitleText, $"{quickStartKey}: QuickStart");

            Window = new UiWindow();

            var items = new TitleScreenCell[] {
                new TitleScreenCell(TitleScreenChoice.Restore),
                new TitleScreenCell(TitleScreenChoice.Generate),
                new TitleScreenCell(TitleScreenChoice.Incarnate),
                new TitleScreenCell(TitleScreenChoice.About),
                new TitleScreenCell(TitleScreenChoice.Options),
                new TitleScreenCell(TitleScreenChoice.Mods),
                new TitleScreenCell(TitleScreenChoice.Exit),
            };
            List = new UiList<TitleScreenChoice>(items);
            List.OnActivated += (_, evt) => RunTitleScreenAction(evt.SelectedCell.Data);

            Window.KeyHints = MakeKeyHints();

            OnKeyBindDown += HandleKeyBindDown;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == ContentKeyFunctions.QuickStart)
            {
                _audio.Play(Protos.Sound.Ok1);
                Finish(new TitleScreenResult(TitleScreenAction.QuickStart));
                args.Handle();
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.AddRange(List.MakeKeyHints());

            keyHints.Add(new(UiKeyHints.Select, UiKeyNames.Cursor));

            return keyHints;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        private void RunTitleScreenAction(TitleScreenChoice selectedChoice)
        {
            _audio.Play(Protos.Sound.Ok1);

            switch (selectedChoice)
            {
                case TitleScreenChoice.Restore:
                    Finish(new TitleScreenResult(TitleScreenAction.RestoreSave));
                    break;
                case TitleScreenChoice.Exit:
                    Finish(new TitleScreenResult(TitleScreenAction.Quit));
                    break;
                case TitleScreenChoice.Options:
                    Finish(new TitleScreenResult(TitleScreenAction.Options));
                    break;
                case TitleScreenChoice.Generate:
                    Finish(new TitleScreenResult(TitleScreenAction.Generate));
                    break;
                default:
                    break;
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(320, 355);
            List.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            TextInfo[0].SetPosition(X + 20, Y + 20);
            TextInfo[1].SetPosition(X + 20, Y + 20 + FontTitleText.LoveFont.GetHeightV(UIScale) + 5);
            TextInfo[2].SetPosition(X + 20, Y + 20 + (FontTitleText.LoveFont.GetHeightV(UIScale) + 5) * 2);
            TextQuickStart.SetPosition(X + 20, Y + Height - 20 - FontTitleText.LoveFont.GetHeightV(UIScale));
            Window.SetPosition(X + 80, (Height - 308) / 2);
            List.SetPosition(Window.X + 40, Window.Y + 48);
        }

        public override void OnQuery()
        {
            Music.Play(Protos.Music.Opening);
        }

        public override void Update(float dt)
        {
            foreach (var text in TextInfo)
                text.Update(dt);
            TextQuickStart.Update(dt);

            Window.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            foreach (var text in TextInfo)
                text.Draw();
            TextQuickStart.Draw();

            Window.Draw();
            List.Draw();

            var bgPicWidth = Window.Width / 5 * 4;
            var bgPicHeight = Window.Height - 80;
            GraphicsEx.SetColor(255, 255, 255, 50);
            AssetG4.Draw(UIScale, Window.X + 160 - bgPicWidth / 2,
                                   Window.Y + Window.Height / 2 - bgPicHeight / 2,
                                   bgPicWidth,
                                   bgPicHeight);
        }

        public override void Dispose()
        {
            foreach (var text in TextInfo)
            {
                text.Dispose();
            }
            TextQuickStart.Dispose();
            Window.Dispose();
            List.Dispose();
        }
    }
}
