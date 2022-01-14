using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.CharaMake
{
    /// <summary>
    /// Still very WIP atm, unsure how the data will be stored. Essentially just a placeholder.
    /// </summary>
    [Localize("Elona.CharaMake.AppearanceSelect")]
    public class CharaMakeAppearanceLayer : CharaMakeLayer
    {
        public abstract record UiAppearanceData()
        {
            public record Done : UiAppearanceData;
        }
        public class AppearanceCell : UiListCell<UiAppearanceData>
        {
            public AppearanceCell(UiAppearanceData data, string text) 
                : base(data, new UiText(text))
            {
            }

            public override void Draw()
            {
                UiText.Draw();
            }
        }

        [Localize] private UiWindow Window;
        private UiTextTopic Category;
        private UiTopicWindow CharaFrame;
        private UiList<UiAppearanceData> List;
        private IAssetDrawable AppearanceDeco;
        public CharaMakeAppearanceLayer()
        {
            AppearanceDeco = new AssetDrawable(Protos.Asset.DecoMirrorA);
            Window = new UiWindow
            {
                KeyHints = MakeKeyHints()
            };
            Category = new UiTextTopic(Loc.GetString("Elona.CharaMake.AppearanceSelect.Topic.Category"));
            List = new UiList<UiAppearanceData>
            {
                new AppearanceCell(new UiAppearanceData.Done(), Loc.GetString("Elona.CharaMake.AppearanceSelect.Done"))
            };
            List.EventOnActivate += ListOnActivate;
            CharaFrame = new UiTopicWindow();
            AddChild(List);
        }

        private void ListOnActivate(object? sender, UiListEventArgs<UiAppearanceData> args)
        {
            switch (args.SelectedCell.Data)
            {
                case UiAppearanceData.Done:
                    Finish(new CharaMakeResult(new Dictionary<string, object>
                    {

                    }));
                    break;
            }
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Port);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(380, 340);
            Category.SetPreferredSize();
            List.SetPreferredSize();
            CharaFrame.SetSize(90, 120);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window, -15);
            Category.SetPosition(Window.X + 30, Window.Y + 35);
            List.SetPosition(Window.X + 30, Window.Y + 65);
            AppearanceDeco.SetPosition(Window.X + Window.Width - 40, Window.Y);
            CharaFrame.SetPosition(Window.X + 230, Window.Y + 70);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            Category.Draw();
            List.Draw();
            AppearanceDeco.Draw();
            CharaFrame.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            Category.Update(dt);
            List.Update(dt);
            AppearanceDeco.Update(dt);
            CharaFrame.Update(dt);
        }

        public override void Dispose()
        {
            base.Dispose();
            List.EventOnActivate -= ListOnActivate;
        }
    }
}
