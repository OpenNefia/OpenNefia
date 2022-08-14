using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Content.CharaAppearance.CharaAppearanceList;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.UI.Element
{
    /// <summary>
    /// Contains a set of <see cref="UiElement"/>s and allows the
    /// user to switch between them like pages. If the contained elements
    /// implement <see cref="IUiPaged"/>, they are also paged internally
    /// within the container.
    /// </summary>
    public sealed class UiPagedContainer : UiElement, IUiPaged
    {
        private class BoundKeyEventFilter : IBoundKeyEventFilter
        {
            public bool FilterEvent(UiElement element, GUIBoundKeyEventArgs evt)
            {
                return evt.Function != EngineKeyFunctions.UINextPage && evt.Function != EngineKeyFunctions.UIPreviousPage;
            }
        }

        private sealed record PageMapping(UiElement Element, int ChildPage);

        private readonly Dictionary<int, PageMapping> _parentPagesToChildPages = new();
        private List<UiElement> _contained = new();
        private IBoundKeyEventFilter _eventFilter = new BoundKeyEventFilter();

        public event PageChangedDelegate? OnPageChanged;

        public int CurrentPage { get; private set; }
        public int PageCount { get; private set; }

        public UiElement? CurrentElement { get; private set; }

        public UiPagedContainer(IEnumerable<UiElement> children)
        {
            CurrentPage = 0;
            SetElements(children);

            EventFilter = UIEventFilterMode.Pass;
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            UserInterfaceManager.ControlFocused = null;
            UserInterfaceManager.ReleaseKeyboardFocus();
            CurrentElement?.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UIPreviousPage)
            {
                PageBackward();
                evt.Handle();
            }
            else if (evt.Function == EngineKeyFunctions.UINextPage)
            {
                PageForward();
                evt.Handle();
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            if (CurrentElement != null)
                keyHints.AddRange(CurrentElement.MakeKeyHints());

            if (PageCount > 1)
            {
                keyHints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIPreviousPage, EngineKeyFunctions.UINextPage }));
            }

            return keyHints;
        }

        private void RemoveKeyFilterRecursive(UiElement elem)
        {
            elem.BoundKeyEventFilters.Remove(_eventFilter);
            foreach (var child in elem.Children)
            {
                RemoveKeyFilterRecursive(child);
            }
        }

        private void AddKeyFilterRecursive(UiElement elem)
        {
            elem.BoundKeyEventFilters.Add(_eventFilter);
            foreach (var child in elem.Children)
            {
                AddKeyFilterRecursive(child);
            }
        }

        public void SetElements(IEnumerable<UiElement> children)
        {
            foreach (var elem in _contained)
            {
                RemoveKeyFilterRecursive(elem);
                RemoveChild(elem);
            }

            _contained = children.ToList();

            foreach (var elem in _contained)
            {
                // Ensure [Child] elements are added before adding the key filter.
                this.AddChildrenRecursive(elem);
                AddKeyFilterRecursive(elem);
            }

            RecalculatePageCount();
        }

        public void RecalculatePageCount()
        {
            _parentPagesToChildPages.Clear();

            CurrentElement = null;
            PageCount = 0;

            foreach (var elem in _contained)
            {
                elem.Visible = false;

                if (elem is IUiPaged elemPaged)
                {
                    for (int childPage = 0; childPage < elemPaged.PageCount + 1; childPage++)
                    {
                        _parentPagesToChildPages.Add(PageCount, new PageMapping(elem, childPage));
                        PageCount++;
                    }
                }
                else
                {
                    _parentPagesToChildPages.Add(PageCount, new PageMapping(elem, 0));
                    PageCount++;
                }
            }

            SetPage(Math.Clamp(CurrentPage, 0, PageCount - 1), playSound: false);
        }

        public bool SetPage(int page, bool playSound)
        {
            if (!_parentPagesToChildPages.TryGetValue(page, out var mapping))
                return false;

            CurrentPage = page;
            CurrentElement = mapping.Element;
            CurrentElement.Visible = true;
            GrabFocus();

            var changed = false;

            if (mapping.Element is IUiPaged elemPaged)
                changed = elemPaged.SetPage(mapping.ChildPage);

            if (playSound && !changed)
                Sounds.Play(Sound.Pop1);

            OnPageChanged?.Invoke(CurrentPage, PageCount);

            return true;
        }

        public bool SetPage(int page) => SetPage(page, true);

        public bool PageBackward()
        {
            return SetPage(CurrentPage - 1);
        }

        public bool PageForward()
        {
            return SetPage(CurrentPage + 1);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            foreach (var elem in _contained)
                elem.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            foreach (var elem in _contained)
                elem.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            foreach (var elem in _contained)
                elem.Update(dt);
        }

        public override void Draw()
        {
            CurrentElement?.Draw();
        }
    }
}