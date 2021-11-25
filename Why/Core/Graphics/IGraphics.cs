using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Why.Core.Graphics
{
    public interface IGraphics
    {
        event Action<WindowResizedEventArgs> OnWindowResized;

        event Action<WindowFocusedEventArgs> OnWindowFocused;
    }
}
