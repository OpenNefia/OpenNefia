using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.CharaInfo
{
    /// <summary>
    /// Contains a <see cref="CharaSheetControl"/> and the pages containing
    /// skill/resistance info.
    /// </summary>
    public sealed class CharaInfoPagesControl : UiElement
    {
        private CharaSheetControl Sheet = new();
        [Child] private UiPagedContainer Pages;

        public event PageChangedDelegate? OnPageChanged
        {
            add => Pages.OnPageChanged += value;
            remove => Pages.OnPageChanged -= value;
        }

        public CharaInfoPagesControl()
        {
            Pages = new UiPagedContainer(new[] { Sheet });

            Pages.OnPageChanged += HandlePageChanged;
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            Pages.GrabFocus();
        }

        public void Initialize(EntityUid charaEntity)
        {
            Sheet.Initialize(charaEntity);
        }

        public void RefreshFromEntity()
        {
            Sheet.RefreshFromEntity();
            Pages.RecalculatePageCount();
        }

        private void HandlePageChanged(int newPage, int newPageCount)
        {
            Pages.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UIPreviousPage)
            {
                Pages.PageBackward();
            }
            else if (evt.Function != EngineKeyFunctions.UINextPage)
            {
                Pages.PageForward();
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            return Pages.MakeKeyHints();
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            Sheet.GetPreferredSize(out size);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Pages.SetPosition(X, Y);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Pages.SetSize(Width, Height);
        }

        public override void Update(float dt)
        {
            Pages.Update(dt);
        }

        public override void Draw()
        {
            Pages.Draw();
        }
    }
}
