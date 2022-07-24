using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// An animatable graphic that is rendered in screenspace.
    /// </summary>
    public interface IGlobalDrawable : IDrawable
    {
        public bool IsFinished { get; }

        public bool CanEnqueue();
        public void OnEnqueue();
        public void OnThemeSwitched();
    }
}
