using OpenNefia.Content.Activity;
using OpenNefia.Content.Hud;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.UI.Hud
{
    public sealed class HudAutoTurnWidget : BaseHudWidget
    {
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IGlobalDrawablesManager _globalDrawables = default!;

        private const int TurnsBetweenAnimationLoops = 15;

        private IAssetInstance _autoTurnIcon = default!;

        private BaseAutoTurnAnim? _autoTurnAnimation;
        public BaseAutoTurnAnim? AutoTurnAnimation
        {
            get => _autoTurnAnimation;
            set
            {
                if (_autoTurnAnimation != null)
                    RemoveChild(_autoTurnAnimation);

                _turnsUntilAnimLoop = 0;
                _isFirstAnimFrame = true;
                _autoTurnAnimation = value;

                if (_autoTurnAnimation != null)
                    UiHelpers.AddChildrenRecursive(this, _autoTurnAnimation);

                SetPreferredSize();
                SetPosition(X, Y);
            }
        }

        private float _turnsUntilAnimLoop = 0;
        private bool _isFirstAnimFrame = true;

        [Child] private UiText UiText = new UiTextShadowed(UiFonts.HUDAutoTurnText, "AUTO TURN");
        [Child] private UiTopicWindow Window = new(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Five);
        [Child] private UiTopicWindow AnimWindow = new(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Five);

        public override void Initialize()
        {
            base.Initialize();
            _autoTurnIcon = Assets.Get(Protos.Asset.AutoTurnIcon);
        }

        public void PassTurn()
        {
            _turnsUntilAnimLoop--;
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(X + 43, Y + 6);
            Window.SetPosition(X, Y);
            AnimWindow.SetPosition(X, Y - 104);
            if (_autoTurnAnimation != null)
                _autoTurnAnimation.SetPosition(X + 2, Y - 102);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            UiText.SetPreferredSize();
            Window.SetSize(Width, Height);
            AnimWindow.SetSize(148, 101);
            if (_autoTurnAnimation != null)
                _autoTurnAnimation.SetSize(148, 101);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = (148, 25);
        }

        public override void Update(float dt)
        {
            if (_autoTurnAnimation == null)
                return;

            if (_isFirstAnimFrame)
            {
                _isFirstAnimFrame = false;
                _autoTurnAnimation.OnFirstFrame();
            }

            _autoTurnAnimation.Update(dt);

            if (_turnsUntilAnimLoop <= 0)
            {
                _turnsUntilAnimLoop = TurnsBetweenAnimationLoops;
                var anim = _autoTurnAnimation.MakeGlobalDrawable();
                _globalDrawables.Enqueue(anim, _autoTurnAnimation.Position);
            }
        }

        public override void Draw()
        {
            // >>>>>>>> shade2/screen.hsp:390 	fontSize 13,01 ...
            Window.Draw();
            UiText.Draw();

            Love.Graphics.SetColor(Color.White);
            var rotation = (_world.State.GameDate.Minute / 4) % 2 * (MathF.PI / 2);
            _autoTurnIcon.Draw(UIScale, X + 18, Y + 12, rotationRads: rotation, centered: true);

            if (_autoTurnAnimation != null)
            {
                AnimWindow.Draw();
                Love.Graphics.SetColor(Color.White);
                _autoTurnAnimation.Draw();
            }
            // <<<<<<<< shade2/screen.hsp:423 		} ..
        }
    }
}