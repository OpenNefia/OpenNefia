using OpenNefia.Content.CharaMake;
using OpenNefia.Content.Input;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaInfo
{
    public sealed class CharaInfoUiLayer : CharaGroupUiLayer
    {
        [Child] private CharaSheet Sheet = new();

        private EntityUid _charaEntity;

        public CharaInfoUiLayer()
        {
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void Initialize(CharaGroupSublayerArgs args)
        {
            _charaEntity = args.CharaEntity;

            Sheet.RefreshFromEntity(_charaEntity);
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs obj)
        {
            if (obj.Function == ContentKeyFunctions.UIPortrait)
            {
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            return base.MakeKeyHints();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Sound.Chara);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            Sheet.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: -10);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Sheet.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Sheet.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            Sheet.Update(dt);
        }

        public override void Draw()
        {
            Sheet.Draw();
        }
    }
}