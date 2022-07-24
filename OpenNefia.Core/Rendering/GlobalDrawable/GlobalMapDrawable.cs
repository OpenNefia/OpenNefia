using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public abstract class BaseGlobalDrawable : BaseDrawable, IGlobalDrawable
    {
        public bool IsFinished { get; protected set; }

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
