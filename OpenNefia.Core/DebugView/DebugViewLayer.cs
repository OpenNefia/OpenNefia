using ImGuiNET;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.DebugView
{
    public interface IDebugViewLayer : IUiLayer
    {
    }

    public class DebugViewLayer : UiLayer, IDebugViewLayer, IRawInputControl
    {
        [Dependency] private readonly IDebugViewManager _debugView = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        private System.Numerics.Vector3 _color;
        private Vector2 _mouseWheel;

        public DebugViewLayer()
        {
            CanControlFocus = true;
            CanKeyboardFocus = true;
            EventFilter = UserInterface.UIEventFilterMode.Pass;
        }

        protected internal override void MouseWheel(GUIMouseWheelEventArgs args)
        {
            _mouseWheel = args.Delta;
        }

        protected internal override void MouseMove(GUIMouseMoveEventArgs args)
        {
            var io = ImGui.GetIO();
            io.MousePos = args.GlobalPosition;
        }

        public bool RawKeyEvent(in GuiRawKeyEvent guiRawEvent)
        {
            var io = ImGui.GetIO();
            var down = guiRawEvent.Action == RawKeyAction.Down || guiRawEvent.Action == RawKeyAction.Repeat;

            if (Keyboard.IsMouseKey(guiRawEvent.Key))
            {
                io.AddMouseButtonEvent((int)guiRawEvent.Key - (int)Keyboard.Key.MouseLeft, down);
            }
            else
            {
                io.AddKeyEvent((ImGuiKey)io.KeyMap[(int)guiRawEvent.Key], down);
            }

            return true;
        }

        public override void OnQuery()
        {
            var io = ImGui.GetIO();
            io.ClearInputKeys();
        }

        public override void OnQueryFinish()
        {
            var io = ImGui.GetIO();
            io.ClearInputKeys();
        }

        public override void Update(float dt)
        {
            var io = ImGui.GetIO();

            io.MouseWheel = _mouseWheel.Y;
            io.MouseWheelH = _mouseWheel.X;

            _mouseWheel = Vector2.Zero;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.MenuItem("Test");
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ImGui.Begin("Test Window"))
            {
                ImGui.Text("yo doods");
                ImGui.ColorEdit3("Clear color", ref _color);

                ImGui.End();
            }
        }

        public override void Draw()
        {
            _debugView.Draw();
        }
    }
}
