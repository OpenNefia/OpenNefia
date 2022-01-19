using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Hud
{
    public interface IBacklog
    {
        public bool IsShowingBacklog { get; }
        void ToggleBacklog(bool visible);
    }
}
