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
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.CharaMake;
using OpenNefia.Core.UI;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.PCCs;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ResourceManagement;

namespace OpenNefia.Content.Charas
{
    [Localize("Elona.Chara.AppearanceLayer")]
    public class CharaAppearanceWindow : UiElement
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

        [Dependency] private readonly IResourceCache _resourceCache = default!;

        [Localize] private UiWindow Window = new();
        private UiTextTopic Category;
        private UiTopicWindow CharaFrame;
        public UiList<UiAppearanceData> List { get; }
        private IAssetDrawable AppearanceDeco;




        private PCCDrawable _pccDrawable;



        public CharaAppearanceWindow()
        {
            IoCManager.InjectDependencies(this);

            AppearanceDeco = new AssetDrawable(Protos.Asset.DecoMirrorA);
            Category = new UiTextTopic(Loc.GetString("Elona.CharaMake.AppearanceSelect.Topic.Category"));
            Window.KeyHints = MakeKeyHints();
            List = new UiList<UiAppearanceData>
            {
                new AppearanceCell(new UiAppearanceData.Done(), Loc.GetString("Elona.CharaMake.AppearanceSelect.Done"))
            };
            CharaFrame = new UiTopicWindow();
            AddChild(List);

            _pccDrawable = SetupPCC();
        }

        private PCCDrawable SetupPCC()
        {
            var allParts = IoCManager.Resolve<IPrototypeManager>()
                .EnumeratePrototypes<PCCPartPrototype>()
                .GroupBy(part => part.PCCPartType)
                .Select(group => group.FirstOrDefault())
                .WhereNotNull()
                .Select(part => new PCCPart(part.PCCPartType, part.ImagePath, Color.White));

            var pccDrawable = new PCCDrawable(allParts);
            pccDrawable.RebakeImage(_resourceCache);
            return pccDrawable;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = new(380, 340);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            Category.SetPreferredSize();
            List.SetPreferredSize();
            CharaFrame.SetSize(90, 120);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
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



            _pccDrawable.Frame = Math.Clamp(((int)_frame / 4) % 4, 0, 3);
            _pccDrawable.Direction = (PCCDirection)Math.Clamp(((int)_frame / 16) % 4, 0, 3);
            _pccDrawable.Draw(CharaFrame.X + 46 - 24, CharaFrame.Y + 59 - 24, 2.0f, 2.0f);
        }

        private int _pccFrame = 0;
        private float _frame = 0f;

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            Category.Update(dt);
            List.Update(dt);
            AppearanceDeco.Update(dt);
            CharaFrame.Update(dt);


            _frame += dt * 50;

            _pccDrawable.Update(dt);
        }
    }
}