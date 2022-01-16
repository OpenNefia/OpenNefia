using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Hud
{
    public interface IHudLayer : IUiLayer
    {
        public IHudMessageWindow MessageWindow { get; }
        void ToggleBacklog(bool visible);
        void Initialize();
    }
}