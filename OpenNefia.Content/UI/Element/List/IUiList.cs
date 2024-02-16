using OpenNefia.Core;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.UI.Element.List
{
    public interface IUiList : IUiInput, ILocalizable
    {
        bool HighlightSelected { get; set; }
        bool SelectOnActivate { get; set; }
        int SelectedIndex { get; }

        bool CanSelect(int index);
        void IncrementIndex(int delta);
        void Select(int index);
        bool CanActivate(int index);
        void Activate(int index);
    }

    public interface IUiList<T> : IUiList, IReadOnlyList<UiListCell<T>>
    {
        event UiListEventHandler<T>? OnSelected;
        event UiListEventHandler<T>? OnActivated;

        public IUiListCell<T>? SelectedCell { get; }

        void SetCells(IEnumerable<UiListCell<T>> items);
        void CreateAndSetCells(IEnumerable<T> items);
    }
}
