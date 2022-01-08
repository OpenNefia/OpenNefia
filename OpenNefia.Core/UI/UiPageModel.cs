﻿using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using System;
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
    public class UiPageModel<T> : IUiPaged
    {
        private IEnumerable<T> PagedElements;

        private int ItemsPerPage;

        /// <inheritdoc/>
        public int CurrentPage { get; private set; } = 0;

        /// <inheritdoc/>
        public int PageCount => PagedElements.Count() / Math.Max(1, ItemsPerPage);
        
        private List<T> _currentElements = new();
        public IReadOnlyList<T> CurrentElements => _currentElements;

        public event PageChangedDelegate? OnPageChanged;

        public UiPageModel()
        {
            PagedElements = Enumerable.Empty<T>();
        }

        public void Initialize(IEnumerable<T> elements, int itemsPerPage = 16)
        {
            PagedElements = elements;
            ItemsPerPage = itemsPerPage;
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
            CurrentPage = Math.Clamp(page, 0, PageCount);
            UpdateCurrentElements();
            OnPageChanged?.Invoke(page, PageCount);
            return oldPage != CurrentPage;
        }
    }
}