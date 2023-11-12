using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public abstract class BaseMapDrawable : BaseDrawable, IMapDrawable
    {
        public bool IsFinished { get; protected set; }
        public IMap Map { get => GameSession.ActiveMap!; }
        public Vector2 ScreenOffset { get; set; }
        public Vector2i ScreenLocalPos { get; set; }

        public virtual void OnEnqueue()
        {
        }

        public virtual bool CanEnqueue()
        {
            return true;
        }

        public virtual void OnThemeSwitched()
        {
        }

        public virtual void Finish()
        {
            this.IsFinished = true;
        }
    }
}
