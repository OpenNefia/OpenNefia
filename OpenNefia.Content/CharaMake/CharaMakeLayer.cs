using OpenNefia.Content.EntityGen;
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
    public abstract class CharaMakeLayer : UiLayerWithResult<CharaMakeResultSet, CharaMakeUIResult>, ICharaMakeLayer
    { }

    public class CharaMakeLayer<T> : CharaMakeLayer, ICharaMakeLayer<T>
        where T : ICharaMakeResult
    {
        [Dependency] protected readonly IEntityManager EntityManager = default!;

        protected IAssetInstance AssetBG;
        protected IAssetInstance[] AssetWindows;
        protected IAssetInstance CurrentWindowBG;

        protected CharaMakeResultSet Results = default!;

        [Child][Localize] protected CharaMakeCaption Caption;
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

            Caption = new CharaMakeCaption();

            OnKeyBindDown += HandleKeyBindDown;
        }

        protected virtual void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Handled)
                return;

            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Finish(new CharaMakeUIResult(null, CharaMakeStep.GoBack));
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIDown || args.Function == EngineKeyFunctions.UIUp)
            {
                UiMoveCount++;
                CurrentWindowBG = AssetWindows[(UiMoveCount / 4) % 4];
                args.Handle();
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

        public override void Initialize(CharaMakeResultSet args)
        {
            Results = args;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Caption.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Caption.SetPosition(20, 30);
        }

        public override void Draw()
        {
            AssetBG.DrawUnscaled(0, 0, Love.Graphics.GetWidth(), Love.Graphics.GetHeight());
            Caption.Draw();
        }

        public override void Dispose()
        {
            OnKeyBindDown -= HandleKeyBindDown;
        }
    }
}
