using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
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
        private sealed record PageMapping(UiElement Element, int ChildPage);

        private readonly Dictionary<int, PageMapping> _parentPagesToChildPages = new();
        private List<UiElement> _contained = new();

        public event PageChangedDelegate? OnPageChanged;

        public int CurrentPage { get; private set; }
        public int PageCount { get; private set; }

        public UiElement? CurrentElement { get; private set; }

        public UiPagedContainer(IEnumerable<UiElement> children)
        {
            CurrentPage = 0;
            SetElements(children);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            CurrentElement?.GrabFocus();
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

        public void SetElements(IEnumerable<UiElement> children)
        {
            foreach (var elem in _contained)
            {
                RemoveChild(elem);
            }

            _contained = children.ToList();

            foreach (var elem in _contained)
            {
                AddChild(elem);
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
                if (elem is IUiPaged elemPaged)
                {
                    for (int childPage = 0; childPage < elemPaged.PageCount; childPage++)
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

            if (mapping.Element is IUiPaged elemPaged)
                elemPaged.SetPage(mapping.ChildPage);

            if (playSound)
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