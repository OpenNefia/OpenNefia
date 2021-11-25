using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Why.Core.Graphics
{
    public class LoveGraphics : IGraphics
    {
        public event Action<WindowResizedEventArgs>? OnWindowResized;

        public event Action<WindowFocusedEventArgs>? OnWindowFocused;
    }
}
