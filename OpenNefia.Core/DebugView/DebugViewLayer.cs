using ImGuiNET;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
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

    public class DebugViewLayer : UiLayer, IDebugViewLayer
    {
        [Dependency] private readonly IDebugViewManager _debugView = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        private System.Numerics.Vector3 _color;

        public DebugViewLayer()
        {
            CanControlFocus = true;
            CanKeyboardFocus = true;
            EventFilter = UserInterface.UIEventFilterMode.Pass;
        }

        public override void Update(float dt)
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.MenuItem("Test");
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            ImGui.SetNextWindowPos(new(200, 200));
            ImGui.SetNextWindowSize(_graphics.WindowSize);

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
