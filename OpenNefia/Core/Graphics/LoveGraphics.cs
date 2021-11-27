using System;
using Love;
using Timer = Love.Timer;

namespace OpenNefia.Core.Graphics
{
    public class LoveGraphics : IGraphics
    {
        public event Action<WindowResizedEventArgs>? OnWindowResized;

        public event Action<WindowFocusedEventArgs>? OnWindowFocused;

        public void Initialize()
        {
            var bootConfig = new BootConfig()
            {
                WindowTitle = "OpenNefia",
                WindowDisplay = 0,
                WindowMinWidth = 800,
                WindowMinHeight = 600,
                WindowVsync = true,
                WindowResizable = true,
                DefaultRandomSeed = 0
            };

            Boot.Init(bootConfig);
            Timer.Step();
        }

        public void Shutdown()
        {
        }
    }
}
