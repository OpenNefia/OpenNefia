using OpenNefia.Core;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.UI.Element.List
{
    public interface IUiList<T> : IList<IUiListCell<T>>, IUiInput, ILocalizable
    {
        public bool HighlightSelected { get; set; }
        public bool SelectOnActivate { get; set; }

        event UiListEventHandler<T>? EventOnSelect;
        event UiListEventHandler<T>? EventOnActivate;

        public int SelectedIndex { get; }
        public IUiListCell<T> SelectedCell { get; }

        bool CanSelect(int index);
        void IncrementIndex(int delta);
        void Select(int index);
        bool CanActivate(int index);
        void Activate(int index);
    }
}
