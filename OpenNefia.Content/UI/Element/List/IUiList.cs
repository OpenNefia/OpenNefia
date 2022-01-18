using OpenNefia.Core;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.UI.Element.List
{
    public interface IUiList : IUiInput, ILocalizable
    {
        public bool HighlightSelected { get; set; }
        public bool SelectOnActivate { get; set; }
        public int SelectedIndex { get; }

        bool CanSelect(int index);
        void IncrementIndex(int delta);
        void Select(int index);
        bool CanActivate(int index);
        void Activate(int index);

        void Clear(bool dispose);
    }

    public interface IUiList<T> : IUiList, IList<UiListCell<T>>
    {
        event UiListEventHandler<T>? OnSelected;
        event UiListEventHandler<T>? OnActivated;

        public IUiListCell<T>? SelectedCell { get; }

        void SetAll(IEnumerable<UiListCell<T>> items, bool dispose = true);
    }
}
