using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public interface IMapDrawable : IDrawable
    {
        public bool IsFinished { get; }
        public IMap Map { get; }
        public Vector2i ScreenLocalPos { get; set; }

        public bool CanEnqueue();
        public void OnEnqueue();
        public void OnThemeSwitched();
    }
}
