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
    public class UiPageModel<T> : UiElement
    {
        private UiText PageText;

        private IEnumerable<T> PagedElements;
        private int ItemsPerPage;
        private int? CurrentPage;
        private UiWindow? Window;
        public int PageCount => (PagedElements.Count() / Math.Max(1, ItemsPerPage));

        public delegate void PageChanged();
        public event PageChanged? OnPageChanged;

        public UiPageModel()
        {
            PagedElements = Enumerable.Empty<T>();
            PageText = new UiText(UiFonts.WindowPage);
        }

        public void Initialize(IEnumerable<T> elements, int itemsPerPage = 15)
        {
            PagedElements = elements;
            ItemsPerPage = itemsPerPage;
            ChangePage(0);
        }

        public IEnumerable<T> GetCurrentElements()
        {
            var en = PagedElements.Skip(ItemsPerPage * CurrentPage ?? 0);
            return en.Take(ItemsPerPage);
        }

        public void PageForward()
        {
            if (!CurrentPage.HasValue)
                return;
            if (PageCount > CurrentPage)
            {
                ChangePage((CurrentPage ?? 0) + 1);
            }
        }

        public void PageBackward()
        {
            if (!CurrentPage.HasValue)
                return;
            if (CurrentPage > 0)
            {
                ChangePage((CurrentPage ?? 0) - 1);
            }
        }

        public void ChangePage(int page)
        {
            if (CurrentPage.HasValue)
                Sounds.Play(Prototypes.Protos.Sound.Pop1);
            CurrentPage = page;
            UpdatePageText();
            OnPageChanged?.Invoke();
        }

        public void SetWindow(UiWindow window)
        {
            Window = window;
            SetPosition(window.X, window.Y);
        }

        private void UpdatePageText()
        {
            PageText.Text = PageCount > 1 ? $"Page.{CurrentPage + 1}/{PageCount + 1}" : string.Empty;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            if (Window != null)
                PageText.SetPosition(Window.X + Window.Width - 85, Window.Y + Window.Height - 68);
        }

        public override void Draw()
        {
            base.Draw();
            if (Window != null)
                PageText.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            PageText.Update(dt);
        }
    }
}