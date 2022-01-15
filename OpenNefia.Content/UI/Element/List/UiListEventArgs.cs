﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element.List
{
    public class UiListEventArgs<T>
    {
        public int SelectedIndex { get; }
        public IUiListCell<T> SelectedCell { get; }
        public bool Handled { get; private set; }

        public UiListEventArgs(IUiListCell<T> choice, int index)
        {

            SelectedCell = choice;
            SelectedIndex = index;
        }

        public void Handle()
        {
            Handled = true;
        }
    }
}
