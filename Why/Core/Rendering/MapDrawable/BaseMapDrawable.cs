using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public abstract class BaseMapDrawable : BaseDrawable, IMapDrawable
    {
        public bool IsFinished { get; protected set; }
        public int ScreenLocalX { get; set; }
        public int ScreenLocalY { get; set; }

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
