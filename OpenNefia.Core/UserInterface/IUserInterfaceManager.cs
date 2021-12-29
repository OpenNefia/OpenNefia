using OpenNefia.Core.Maps;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Core.UserInterface
{
    public interface IUserInterfaceManager
    {
        /// <summary>
        /// A control can have "keyboard focus" separate from ControlFocused, obtained when calling
        /// Control.GrabKeyboardFocus. Corresponding events in Control are KeyboardFocusEntered/Exited
        /// </summary>
        UiElement? KeyboardFocused { get; }

        /// <summary>
        /// A control gets "ControlFocused" when a mouse button (or any KeyBinding which has CanFocus = true) is
        /// pressed down on the control. While it is focused, it will receive mouse hover events and the corresponding
        /// keyup event if it still has focus when that occurs (it will NOT receive the keyup if focus has
        /// been taken by another control). Focus is removed when a different control takes focus
        /// (such as by pressing a different mouse button down over a different control) or when the keyup event
        /// happens. When focus is lost on a control, it always fires Control.ControlFocusExited.
        /// </summary>
        UiElement? ControlFocused { get; set; }

        UiElement? CurrentlyHovered { get; }

        /// <summary>
        ///     Gets the mouse position in UI space, accounting for <see cref="UIScale"/>.
        /// </summary>
        ScreenCoordinates MousePositionScaled { get; }

        public IList<UiLayer> ActiveLayers { get; }

        public UiLayer? CurrentLayer { get; }

        void GrabKeyboardFocus(UiElement control);
        void ReleaseKeyboardFocus(UiElement control);

        void DrawLayers();
        bool IsQuerying(UiLayer layer);
        void PopLayer(UiLayer layer);
        void PushLayer(UiLayer layer);
        void UpdateLayers(FrameEventArgs frame);
        bool IsInActiveLayerList(UiLayer layer);

        UiResult<T> Query<T>(IUiLayerWithResult<T> layer) where T : class;
    }
}