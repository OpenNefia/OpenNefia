using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeLayer : UiLayerWithResult<CharaMakeData, CharaMakeResult>, ICharaMakeLayer
    {
        protected class AttributeIcon : UiElement
        {
            private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>
            {
                { "Elona.StatStrength", "0" },
                { "Elona.StatConstitution", "1" },
                { "Elona.StatDexterity", "2" },
                { "Elona.StatPerception", "3" },
                { "Elona.StatLearning", "4" },
                { "Elona.StatWill", "5" },
                { "Elona.StatMagic", "6" },
                { "Elona.StatCharisma", "7" }

            };
            private IAssetInstance AssetAttributeIcons;
            private string Type;

            public AttributeIcon(string type)
            {
                AssetAttributeIcons = Assets.Get(AssetPrototypeOf.AttributeIcons);
                Type = type;
            }

            public override void Draw()
            {
                base.Draw();
                GraphicsEx.SetColor(Color.White);
                if (_attributes.TryGetValue(Type, out var iconId))
                    AssetAttributeIcons.DrawRegion($"{iconId ?? "2"}", X, Y, centered: true);
            }

            public override void GetPreferredSize(out Vector2i size)
            {
                size = new Vector2i(10, 10);
            }
        }

        protected IAssetInstance AssetBG;
        private IAssetInstance[] AssetWindows;

        protected CharaMakeData Data = default!;
        private UiTopicWindow CaptionWindow;
        [Localize]
        protected UiText Caption;

        protected readonly string[] AttributeIds = new[]
        {
            "Elona.StatStrength",
            "Elona.StatConstitution",
            "Elona.StatDexterity",
            "Elona.StatPerception",
            "Elona.StatLearning",
            "Elona.StatWill",
            "Elona.StatMagic",
            "Elona.StatCharisma"
        };

        public CharaMakeLayer()
        {
            AssetBG = Assets.Get(AssetPrototypeOf.Void);

            AssetWindows = new[]
            {
                Assets.Get(AssetPrototypeOf.G1),
                Assets.Get(AssetPrototypeOf.G2),
                Assets.Get(AssetPrototypeOf.G3),
                Assets.Get(AssetPrototypeOf.G4)
            };

            CaptionWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Five, UiTopicWindow.WindowStyleKind.One);
            Caption = new UiTextOutlined(UiFonts.WindowTitle);

            OnKeyBindDown += OnKeyDown;
        }

        private void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Finish(new CharaMakeResult(new Dictionary<string, object>(), CharaMakeStep.GoBack));
            }
        }

        protected UiContainer MakeSkillContainer(string attr, string text, Color? color = null)
            => MakeSkillContainer(attr, new UiText { Text = text, Color = color ?? Color.Black });
        protected UiContainer MakeSkillContainer(string attr, UiText text)
        {
            var cont = new UiHorizontalContainer();
            cont.AddLayout(LayoutType.XOffset, 7);
            cont.AddElement(new AttributeIcon(attr), LayoutType.YOffset, 3);
            cont.AddLayout(LayoutType.XOffset, -7);

            cont.AddElement(text);

            return cont;
        }

        public override void Initialize(CharaMakeData args)
        {
            Data = args;
        }

        protected void Center(UiWindow window, int yOffset = 20)
        {
            window.SetPosition((Width - window.Width) / 2, ((Height - window.Height) / 2) + yOffset);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Caption.SetPreferredSize();
            CaptionWindow.SetSize(Caption.TextWidth + 50, 27);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            CaptionWindow.SetPosition(18, 25);
            Caption.SetPosition(43, 30);
        }

        public override void Draw()
        {
            AssetBG.Draw(X, Y, Width, Height);
            CaptionWindow.Draw();
            Caption.Draw();
        }

        //will be used to actually make the change to the character after creation
        public virtual void ApplyStep()
        {

        }

        public override void Dispose()
        {
            OnKeyBindDown -= OnKeyDown;
        }
    }
}
