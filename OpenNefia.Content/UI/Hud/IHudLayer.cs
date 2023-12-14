using OpenNefia.Content.Hud;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.UI.Hud
{
    public interface IHudLayer : IUiLayer
    {
        public IHudMessageWindow MessageWindow { get; }
        public IBacklog Backlog { get; }

        /// <summary>
        /// Portion of the game window not covered by the HUD, in virtual pixels.
        /// </summary>
        public UIBox2 GameBounds { get; }

        /// <summary>
        /// Portion of the game window not covered by the HUD, in physical pixels.
        /// </summary>
        public UIBox2 GamePixelBounds { get; }

        void Initialize();
        void ClearWidgets();
        void RefreshWidgets();
        bool TryGetWidget<T>([NotNullWhen(true)] out T? widget, [NotNullWhen(true)] out WidgetInstance? instance)
            where T : class, IHudWidget;
        bool TryGetWidget<T>([NotNullWhen(true)] out T? widget)
            where T : class, IHudWidget;
        bool TryGetWidgetInstance<T>([NotNullWhen(true)] out WidgetInstance? instance)
            where T : class, IHudWidget;
    }
}