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
        private IUiElement? Parent;
        private UiText PageText;

        public UiPageText(IUiElement? parent = null)
        {
            Parent = parent;
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
            size = Parent != null ? Parent.PixelSize : Vector2i.Zero;
        }

        public override void SetPosition(int x, int y)
        {
            if (Parent != null)
            {
                x = Parent.X;
                y = Parent.Y;
            }

            base.SetPosition(x, y);
            PageText.SetPosition(X + Width - 85, Y + Height - 68);
        }

        public override void Draw()
        {
            if (Parent == null)
                return;

            PageText.Draw();
        }

        public override void Update(float dt)
        {
            if (Parent == null)
                return;

            PageText.Update(dt);
        }
    }
}
