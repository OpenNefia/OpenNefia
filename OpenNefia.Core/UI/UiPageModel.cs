using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    /// <summary>
    /// Data structure for displaying a large list of elements in a sliding window
    /// of equally-sized chunks.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // TODO implement a sane mutable interface like IList<T>
    public class UiPageModel<T> : IUiPaged, IReadOnlyList<T>
    {
        private IEnumerable<T> PagedElements;

        public int ItemsPerPage { get; }
        public bool WrapPageCount { get; }

        /// <inheritdoc/>
        public int CurrentPage { get; private set; } = 0;

        /// <inheritdoc/>
        public int PageCount => (int)Math.Ceiling(PagedElements.Count() / (float)Math.Max(1, ItemsPerPage));

        private List<T> _currentElements = new();

        /// <summary>
        /// List of elements that form the current page. This is a window into
        /// the entire page model's list of elements.
        /// </summary>
        public IReadOnlyList<T> CurrentElements => _currentElements;

        /// <inheritdoc/>
        public event PageChangedDelegate? OnPageChanged;

        public UiPageModel(int itemsPerPage = 16, bool wrap = true)
        {
            PagedElements = Enumerable.Empty<T>();
            ItemsPerPage = itemsPerPage;
            WrapPageCount = wrap;
        }

        public void SetElements(IEnumerable<T> elements)
        {
            PagedElements = elements;
            SetPage(0);
        }

        private void UpdateCurrentElements()
        {
            var en = PagedElements.Skip(ItemsPerPage * CurrentPage);
            _currentElements = en.Take(ItemsPerPage).ToList();
        }

        /// <inheritdoc/>
        public bool PageForward()
        {
            return SetPage(CurrentPage + 1);
        }

        /// <inheritdoc/>
        public bool PageBackward()
        {
            return SetPage(CurrentPage - 1);
        }

        /// <inheritdoc/>
        public bool SetPage(int page)
        {
            var oldPage = CurrentPage;
            if (WrapPageCount)
                CurrentPage = MathHelper.Wrap(page, 0, PageCount - 1);
            else
                CurrentPage = int.Clamp(page, 0, PageCount - 1);
            UpdateCurrentElements();
            OnPageChanged?.Invoke(CurrentPage, PageCount);
            return oldPage != CurrentPage;
        }

        public T this[int index] => ((IReadOnlyList<T>)_currentElements)[index];

        public int Count => ((IReadOnlyCollection<T>)_currentElements).Count;

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_currentElements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_currentElements).GetEnumerator();
        }
    }
}