using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UserInterface
{
    public interface IBoundKeyEventFilter
    {
        bool FilterEvent(UiElement element, GUIBoundKeyEventArgs evt);
    }
}
