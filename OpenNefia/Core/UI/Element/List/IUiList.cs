using OpenNefia.Core.Data.Types;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.UI.Element.List
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
