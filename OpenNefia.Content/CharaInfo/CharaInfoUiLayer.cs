using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaInfo
{
    public sealed class CharaInfoUiLayer : CharaGroupUiLayer
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        [Child] private UiKeyHintBar KeyHintBar = new();
        [Child] private CharaInfoPagesControl CharaInfoPages = new();

        private EntityUid _charaEntity;

        public CharaInfoUiLayer()
        {
            CharaInfoPages.OnPageChanged += HandlePagesPageChanged;

            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            CharaInfoPages.GrabFocus();
        }

        public override void Initialize(CharaGroupSublayerArgs args)
        {
            _charaEntity = args.CharaEntity;

            CharaInfoPages.Initialize(_charaEntity);
            CharaInfoPages.RefreshFromEntity();
        }

        private void HandlePagesPageChanged(int newPage, int newPageCount)
        {
            UpdateKeyHintBar();
        }

        private void UpdateKeyHintBar()
        {
            KeyHintBar.Text = UserInterfaceManager.FormatKeyHints(MakeKeyHints());
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UICancel)
            {
                // TODO wear equipment flag
                Finish(new());
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(new LocaleKey("Elona.CharaSheet.KeyHint.BlessingAndHex"), UiKeyNames.Cursor));
            keyHints.AddRange(CharaInfoPages.MakeKeyHints());
            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return keyHints;
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Sound.Chara);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            CharaInfoPages.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: -10);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            KeyHintBar.SetSize(_graphics.WindowSize.X - 240, 16);
            CharaInfoPages.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            KeyHintBar.SetPosition(240, 0);
            CharaInfoPages.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            KeyHintBar.Update(dt);
            CharaInfoPages.Update(dt);
        }

        public override void Draw()
        {
            KeyHintBar.Draw();
            CharaInfoPages.Draw();
        }
    }
}