﻿using Love;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.UI;
using Vector2 = OpenNefia.Core.Maths.Vector2;

namespace OpenNefia.Core.Graphics
{
    public class HeadlessGraphics : Love.Scene, IGraphics
    {
        public event Action<WindowResizedEventArgs>? OnWindowResized;
        public event Action<WindowFocusedEventArgs>? OnWindowFocused;
        public new event Action<KeyPressedEventArgs>? OnKeyPressed;
        public new event Action<KeyPressedEventArgs>? OnKeyReleased;
        public new event Action<TextEditingEventArgs>? OnTextEditing;
        public new event Action<TextInputEventArgs>? OnTextInput;
        public new event Action<MouseMovedEventArgs>? OnMouseMoved;
        public new event Action<MousePressedEventArgs>? OnMousePressed;
        public new event Action<MousePressedEventArgs>? OnMouseReleased;

        private Love.Canvas TargetCanvas = default!;

        public void Initialize()
        {
            // TODO: Much of Love.Graphics requires the window to be initialized
            // and visible.
            var bootConfig = new BootConfig()
            {
                WindowTitle = Engine.Title,
                WindowDisplay = 0,
                WindowMinWidth = 800,
                WindowMinHeight = 600,
                WindowVsync = true,
                WindowResizable = true,
                DefaultRandomSeed = 0
            };

            Love.Boot.Init(bootConfig);
            Love.Timer.Step();
            Love.Boot.SystemStep(this);
        }

        public void Shutdown()
        {
        }

        public void BeginDraw()
        {
        }

        public void EndDraw()
        {
        }
    }
}