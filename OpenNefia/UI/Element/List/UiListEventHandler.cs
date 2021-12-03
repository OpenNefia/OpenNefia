using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Element.List
{
    public delegate void UiListEventHandler<T>(object? sender, UiListEventArgs<T> e);
}
