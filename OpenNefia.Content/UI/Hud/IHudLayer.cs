using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;

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

        void Initialize();
        void ClearWidgets();
    }
}