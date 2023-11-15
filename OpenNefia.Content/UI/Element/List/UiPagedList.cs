using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.UI.Element.List
{
    public class UiPagedList<T> : UiList<T>, IUiPaged
    {
        private UiPageModel<UiListCell<T>> _pageModel;

        public override IReadOnlyList<UiListCell<T>> DisplayedCells => _pageModel.CurrentElements;

        public int CurrentPage => _pageModel.CurrentPage;
        public int PageCount => _pageModel.PageCount;
        public int ItemsPerPage => _pageModel.ItemsPerPage;

        public IUiElement? PageTextElement
        {
            get => PageText.PageTextParent;
            set => PageText.PageTextParent = value;
        }

        public Vector2 PageTextOffset
        {
            get => PageText.TextOffset;
            set => PageText.TextOffset = value;
        }

        public Vector2 PageTextPixelOffset => PageText.TextPixelOffset;

        [Child] private UiPageText PageText;

        public event PageChangedDelegate? OnPageChanged
        {
            add => _pageModel.OnPageChanged += value;
            remove => _pageModel.OnPageChanged -= value;
        }

        public UiPagedList(int itemsPerPage = 16, IUiElement? elementForPageText = null, Vector2? textOffset = null) : base()
        {
            _pageModel = new UiPageModel<UiListCell<T>>(itemsPerPage);
            _pageModel.SetElements(AllCells);
            PageText = new UiPageText(elementForPageText)
            {
                TextOffset = textOffset ?? default
            };

            OnPageChanged += PageText.UpdatePageText;
            OnPageChanged += HandlePageChanged;
        }

        private void HandlePageChanged(int newPage, int newPageCount)
        {
            if (_pageModel.CurrentElements.Count == 0)
                return;

            var clampedIndex = Math.Clamp(SelectedIndex, 0, _pageModel.CurrentElements.Count - 1);
            Select(clampedIndex);
        }

        protected override void UpdateDisplayedCells(bool setSize)
        {
            var oldPage = _pageModel.CurrentPage;
            _pageModel.SetElements(AllCells);
            _pageModel.SetPage(oldPage);

            base.UpdateDisplayedCells(setSize);
        }

        protected override void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.HandleKeyBindDown(args);
            
            if (args.Function == EngineKeyFunctions.UIPreviousPage)
            {
                PageBackward();
            }
            else if (args.Function == EngineKeyFunctions.UINextPage)
            {
                PageForward();
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            if (PageCount > 1)
            {
                keyHints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIPreviousPage, EngineKeyFunctions.UINextPage }));
            }

            return keyHints;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = Vector2.Zero;
            var cellCount = Math.Clamp(AllCells.Count, 0, ItemsPerPage);

            for (int index = 0; index < cellCount; index++)
            {
                var cell = AllCells[index];
                cell.GetPreferredSize(out var cellSize);
                size.X = MathF.Max(size.X, cellSize.X);
                size.Y += MathF.Max(cellSize.Y, ItemHeight);
            }
        }

        public void SelectInAllPages(int index)
        {
            var page = index / ItemsPerPage;
            index = index % ItemsPerPage;
            SetPage(page);
            Select(index);
        }

        public bool SetPage(int page, bool playSound)
        {
            var changed = _pageModel.SetPage(page);
            if (changed)
            {
                if (playSound)
                    Sounds.Play(Sound.Pop1);
                UpdateAllCells();
            }
            return changed;
        }

        public bool SetPage(int page)
        {
            return SetPage(page, true);
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

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            PageText.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            PageText.SetPosition(x, y);
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
