using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.Hud
{
    public interface IHudWidget : IDrawable
    {
        /// <summary>
        /// Will be used for letting the user move the widget around
        /// </summary>
        public float PosX { get; set; }
        public float PosY { get; set; }
        public bool Movable { get; }
        void RefreshWidget();
        void Initialize();
    }
}