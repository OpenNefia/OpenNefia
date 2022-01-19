using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Hud
{
    public interface IHudLayer : IUiLayer
    {
        public IHudMessageWindow MessageWindow { get; }
        public IBacklog Backlog { get; }
        public UIBox2i GameBounds { get; }
        void Initialize();
        void ClearWidgets();
    }
}