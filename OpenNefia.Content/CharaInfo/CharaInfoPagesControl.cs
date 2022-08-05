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
    /// Contains a <see cref="CharaSheetControl"/> and the <see cref="SkillsListControl"/> containing
    /// skill/resistance info.
    /// </summary>
    public sealed class CharaInfoPagesControl : UiElement
    {
        public CharaSheetControl CharaSheet { get; } = new();
        public SkillsListControl SkillsList { get; } = new();
        [Child] public UiPagedContainer Pages { get; }

        public CharaInfoPagesControl()
        {
            Pages = new UiPagedContainer(new UiElement[] { CharaSheet, SkillsList });

            Pages.OnPageChanged += HandlePageChanged;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            Pages.GrabFocus();
        }

        public void Initialize(EntityUid charaEntity)
        {
            CharaSheet.Initialize(charaEntity);
            SkillsList.Initialize(charaEntity);
        }

        public void RefreshFromEntity()
        {
            CharaSheet.RefreshFromEntity();
            SkillsList.RefreshFromEntity();
            Pages.RecalculatePageCount();
        }

        private void HandlePageChanged(int newPage, int newPageCount)
        {
            Pages.GrabFocus();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            return Pages.MakeKeyHints();
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            CharaSheet.GetPreferredSize(out size);
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
