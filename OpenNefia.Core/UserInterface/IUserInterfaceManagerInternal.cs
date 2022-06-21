using OpenNefia.Core.Input;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Wisp;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.UserInterface
{
    internal interface IUserInterfaceManagerInternal : IUserInterfaceManager
    {
        void Initialize();
        void InitializeTesting();

        void Shutdown();

        /// <returns>True if a UI control was hit and the key event should not pass through past UI.</returns>
        bool HandleCanFocusDown(
            ScreenCoordinates pointerPosition,
            [NotNullWhen(true)] out (UiElement control, Vector2i rel)? hitData);

        void HandleCanFocusUp(); 
        
        void KeyBindDown(BoundKeyEventArgs args);

        void KeyBindUp(BoundKeyEventArgs args);

        void MouseMove(MouseMoveEventArgs mouseMoveEventArgs);

        void MouseWheel(MouseWheelEventArgs args);

        void TextEntered(TextEventArgs textEvent);

        void ControlHidden(UiElement control);

        void ControlRemovedFromTree(UiElement control);

        void RemoveModal(UiElement modal);

        Vector2? CalcRelativeMousePositionFor(UiElement control, ScreenCoordinates mousePos);

        void ReleaseKeyboardFocus();

        void PushLayer(UiLayer layer);

        void PopLayer(UiLayer layer);
    }
}