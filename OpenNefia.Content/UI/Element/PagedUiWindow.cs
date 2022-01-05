using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element
{
    public class PagedUiWindow : UiWindow
    {
        private UiText PageText;

        private IEnumerable<UiElement> PagedElements;
        private int ItemsPerPage;
        private int CurrentPage;
        public int PageCount => (PagedElements.Count() / Math.Max(1, ItemsPerPage));

        public delegate void PageChanged();
        public event PageChanged? OnPageChanged;

        public PagedUiWindow()
        {
            PagedElements = Enumerable.Empty<UiElement>();
            PageText = new UiText(UiFonts.WindowPage);
        }

        public void Initialize(IEnumerable<UiElement> elements, int itemsPerPage = 15)
        {
            PagedElements = elements;
            ItemsPerPage = itemsPerPage;
            ChangePage(0);
        }

        public IEnumerable<UiElement> GetCurrentElements()
        {
            var en = PagedElements.Skip(ItemsPerPage * CurrentPage);
            return en.Take(ItemsPerPage);
        }

        public void PageForward()
        {
            if (PageCount > CurrentPage)
            {
                ChangePage(CurrentPage + 1);
            }
        }

        public void PageBackward()
        {
            if (CurrentPage >= 1)
            {
                ChangePage(CurrentPage - 1);
            }
        }

        public void ChangePage(int page)
        {
            Sounds.Play(Prototypes.Protos.Sound.Pop1);
            CurrentPage = page;
            UpdatePageText();
            OnPageChanged?.Invoke();
        }

        private void UpdatePageText()
        {
            //Loc.GetString("WindowPage")
            PageText.Text = PageCount > 1 ? $"Page.{CurrentPage + 1}/{PageCount + 1}" : string.Empty;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            PageText.SetPosition(X + Width - 85, y + Height - 68);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
        }

        public override void Draw()
        {
            base.Draw();
            PageText.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            PageText.Update(dt);
        }
    }
}