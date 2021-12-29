using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.UserInterface
{
    internal interface IUserInterfaceManagerInternal : IUserInterfaceManager
    {        
        /// <returns>True if a UI control was hit and the key event should not pass through past UI.</returns>
        bool HandleCanFocusDown(
            ScreenCoordinates pointerPosition,
            [NotNullWhen(true)] out (UiElement control, Vector2i rel)? hitData);

        void HandleCanFocusUp();

        Vector2? CalcRelativeMousePositionFor(UiElement control, ScreenCoordinates mousePos);
        void ControlRemovedFromTree(UiElement uiElement);
    }
}