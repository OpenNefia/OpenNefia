using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public interface IMapDrawable : IDrawable
    {
        public bool IsFinished { get; }
        public int ScreenLocalX { get; set; }
        public int ScreenLocalY { get; set; }

        public bool CanEnqueue();
        public void OnEnqueue();
        public void OnThemeSwitched();
    }
}
