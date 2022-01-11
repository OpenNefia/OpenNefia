using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
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

        public IUiElement? PageTextElement
        {
            get => PageText.PageTextParent;
            set => PageText.PageTextParent = value;
        }

        private UiPageText PageText;

        public event PageChangedDelegate? OnPageChanged
        {
            add => _pageModel.OnPageChanged += value;
            remove => _pageModel.OnPageChanged -= value;
        }

        public UiPagedList(int itemsPerPage = 16, IUiElement? elementForPageText = null) : base()
        {
            _pageModel = new UiPageModel<UiListCell<T>>(itemsPerPage);
            _pageModel.SetElements(AllCells);
            PageText = new UiPageText(elementForPageText);

            OnPageChanged += PageText.UpdatePageText;
            OnPageChanged += HandlePageChanged;
        }

        private void HandlePageChanged(int newPage, int newPageCount)
        {
            Select(SelectedIndex);
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

            keyHints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIPreviousPage, EngineKeyFunctions.UINextPage }));

            return keyHints;
        }

        public bool SetPage(int page)
        {
            var changed = _pageModel.SetPage(page);
            if (changed)
            {
                Sounds.Play(Sound.Pop1);
                UpdateAllCells();
            }

            return changed;
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

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            PageText.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
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
