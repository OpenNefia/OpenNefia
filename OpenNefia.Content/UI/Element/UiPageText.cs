using Love;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Utility.FormattedMessage;

namespace OpenNefia.Content.UI.Element
{
    public class UiPageText : UiElement
    {
        public IUiElement? PageTextParent { get; set; }
        private UiText PageText;

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
            PageText.Text = newPageCount > 1 ? $"Page.{newPage + 1}/{newPageCount + 1}" : string.Empty;
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = PageTextParent != null ? PageTextParent.PixelSize : Vector2i.Zero;
        }

        public override void SetSize(int width, int height)
        {
            if (PageTextParent != null)
            {
                width = PageTextParent.Width;
                height = PageTextParent.Height;
            }

            base.SetSize(width, height);
            PageText.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            if (PageTextParent != null)
            {
                x = PageTextParent.X;
                y = PageTextParent.Y;
            }

            base.SetPosition(x, y);
            PageText.SetPosition(X + Width - 85, Y + Height - 68);
        }

        public override void Draw()
        {
            if (PageTextParent == null)
                return;

            PageText.Draw();
            UiUtils.DebugDraw(PageTextParent);
        }

        public override void Update(float dt)
        {
            if (PageTextParent == null)
                return;

            PageText.Update(dt);
        }
    }
}
