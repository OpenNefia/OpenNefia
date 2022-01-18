using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    public class UiPageText : UiElement
    {
        public IUiElement? PageTextParent { get; set; }
        private UiText PageText;
        public Vector2 TextOffset { get; set; }
        public Vector2i TextPixelOffset => (Vector2i)(TextOffset * UIScale);

        public UiPageText(IUiElement? parent = null)
        {
            PageTextParent = parent;
            PageText = new UiText(UiFonts.WindowPage);
        }

        /// <summary>
        /// Updates the page text.
        /// </summary>
        /// <remarks>
        /// Bindable to <see cref="UiPageModel{T}.OnPageChanged"/>.
        /// </remarks>
        public void UpdatePageText(int newPage, int newPageCount)
        {
            PageText.Text = newPageCount > 0 ? $"Page.{newPage + 1}/{newPageCount + 1}" : string.Empty;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = PageTextParent != null ? PageTextParent.PixelSize : Vector2i.Zero;
        }

        public override void SetSize(float width, float height)
        {
            if (PageTextParent != null)
            {
                width = PageTextParent.Width;
                height = PageTextParent.Height;
            }

            base.SetSize(width, height);
            PageText.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            if (PageTextParent != null)
            {
                x = PageTextParent.X;
                y = PageTextParent.Y;
            }

            base.SetPosition(x, y);
            PageText.SetPosition(X + Width - 85 + TextOffset.X, Y + Height - 68 + TextOffset.Y);
        }

        public override void Draw()
        {
            if (PageTextParent == null)
                return;

            PageText.Draw();
        }

        public override void Update(float dt)
        {
            if (PageTextParent == null)
                return;

            PageText.Update(dt);
        }
    }
}
