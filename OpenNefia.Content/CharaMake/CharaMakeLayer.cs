using OpenNefia.Content.Input;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
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
            private const string FallbackIcon = "2";
            private readonly Dictionary<PrototypeId<SkillPrototype>, string> _attributes = new()
            {
                { Protos.Skill.StatStrength, "0" },
                { Protos.Skill.StatConstitution, "1" },
                { Protos.Skill.StatDexterity, "2" },
                { Protos.Skill.StatPerception, "3" },
                { Protos.Skill.StatLearning, "4" },
                { Protos.Skill.StatWill, "5" },
                { Protos.Skill.StatMagic, "6" },
                { Protos.Skill.StatCharisma, "7" }

            };
            private IAssetInstance AssetAttributeIcons;
            private PrototypeId<SkillPrototype>? Type;

            public AttributeIcon(PrototypeId<SkillPrototype>? type)
            {
                AssetAttributeIcons = Assets.Get(Protos.Asset.AttributeIcons);
                Type = type;
            }

            public override void Draw()
            {
                base.Draw();
                GraphicsEx.SetColor(Color.White);
                if (Type != null && _attributes.TryGetValue(Type.Value, out var iconId))
                    AssetAttributeIcons.DrawRegion($"{iconId ?? FallbackIcon}", X, Y, centered: true);
            }

            public override void GetPreferredSize(out Vector2i size)
            {
                size = new Vector2i(10, 10);
            }
        }
        [Dependency] protected readonly IEntityManager _entityManager = default!;
        protected IAssetInstance AssetBG;
        protected IAssetInstance[] AssetWindows;
        protected IAssetInstance CurrentWindowBG;

        protected CharaMakeData Data = default!;
        private UiTopicWindow CaptionWindow;
        [Localize]
        protected UiText Caption;
        private int UiMoveCount;

        public CharaMakeLayer()
        {
            AssetBG = Assets.Get(Protos.Asset.Void);

            AssetWindows = new[]
            {
                Assets.Get(Protos.Asset.G1),
                Assets.Get(Protos.Asset.G2),
                Assets.Get(Protos.Asset.G3),
                Assets.Get(Protos.Asset.G4)
            };
            CurrentWindowBG = AssetWindows[0];

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
            else if (args.Function == EngineKeyFunctions.UIDown || args.Function == EngineKeyFunctions.UIUp)
            {
                UiMoveCount++;
                CurrentWindowBG = AssetWindows[(UiMoveCount / 4) % 4];
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Back, EngineKeyFunctions.UICancel));

            return keyHints;
        }

        protected UiContainer MakeSkillContainer(PrototypeId<SkillPrototype> attr, string text, Color? color = null)
            => MakeSkillContainer(attr, new UiText { Text = text, Color = color ?? Color.Black });
        protected UiContainer MakeSkillContainer(PrototypeId<SkillPrototype> attr, UiText text)
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

        protected void Center(UiElement elem, int yOffset = 20)
        {
            elem.SetPosition((Width - elem.Width) / 2, ((Height - elem.Height) / 2) + yOffset);
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

        public override void Dispose()
        {
            OnKeyBindDown -= OnKeyDown;
        }

        //will be used to actually make the change to the character after creation
        public virtual void ApplyStep(EntityUid entity)
        {
            
        }
    }
}
