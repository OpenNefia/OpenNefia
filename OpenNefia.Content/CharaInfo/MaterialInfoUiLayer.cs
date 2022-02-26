using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaInfo
{
    public class MaterialInfoUiLayer : CharaGroupUiLayer
    {
        [Child] private UiScroll Window = new();

        public MaterialInfoUiLayer()
        {
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void Initialize(CharaGroupSublayerArgs args)
        {
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UICancel)
            {
                Finish(SharedSublayerResult);
            }
        }

        public override void OnQuery()
        {
            Sounds.Play(Sound.Scroll);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(600, 430, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();
        }
    }
}
