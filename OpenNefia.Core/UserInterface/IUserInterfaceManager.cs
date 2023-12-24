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
        void ReleaseKeyboardFocus();
        void ReleaseKeyboardFocus(UiElement control);

        void PushModal(UiElement modal);

        string FormatKeyHints(IEnumerable<UiKeyHint> keyHints);

        void DrawLayers();
        bool IsQuerying(UiLayer layer);
        void PushLayer(UiLayer layer, bool noHaltInput = false);
        void PopLayer(UiLayer layer, bool noHaltInput = false);
        void FrameUpdate(FrameEventArgs frame);
        bool IsInActiveLayerList(UiLayer layer);

        TLayer CreateAndInitializeLayer<TLayer, TArgs, TResult>(TArgs args)
            where TLayer : IUiLayerWithResult<TArgs, TResult>, new()
            where TResult : class;

        TLayer CreateAndInitializeLayer<TLayer, TResult>()
            where TLayer : IUiLayerWithResult<UINone, TResult>, new()
            where TResult : class;

        TLayer CreateAndInitializeLayer<TLayer, TArgs>(TArgs args) 
            where TLayer : IUiLayerWithResult<TArgs, UINone>, new();

        TLayer CreateAndInitializeLayer<TLayer>()
            where TLayer : IUiLayer, new();

        void InitializeLayer<TLayer, TArgs, TResult>(TLayer layer, TArgs args)
            where TLayer : IUiLayerWithResult<TArgs, TResult>
            where TResult : class;

        void InitializeLayer<TLayer>(TLayer layer) where TLayer : IUiLayer;

        UiResult<TResult> Query<TLayer, TArgs, TResult>(TArgs args, QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<TArgs, TResult>, new()
            where TResult : class;

        UiResult<TResult> Query<TLayer, TResult>(QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<UINone, TResult>, new()
            where TResult : class;

        UiResult<UINone> Query<TLayer, TArgs>(TArgs args, QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<TArgs, UINone>, new();

        UiResult<UINone> Query<TLayer>(QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<UINone, UINone>, new();

        UiResult<TResult> Query<TArgs, TResult>(IUiLayerWithResult<TArgs, TResult> layer, QueryLayerArgs? queryArgs = null) where TResult : class;

        UiResult<TResult> Query<TResult, TLayer, TArgs>(TLayer layer, TArgs args, QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<TArgs, TResult>
            where TResult : class;

        // TODO remove after merging with WispManager
        event Action? OnHoveredElementChanged;
    }

    public sealed record QueryLayerArgs(bool NoHaltInput = false);
}