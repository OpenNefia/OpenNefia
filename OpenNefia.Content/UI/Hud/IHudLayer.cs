using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Hud
{
    public interface IHudLayer : IUiLayer
    {
        public IHudMessageWindow MessageWindow { get; }
        public Vector2i HudScreenOffset { get; }
        void ToggleBacklog(bool visible);
        void UpdateTime();
        void UpdateMinimap();
        void Initialize();
    }
}