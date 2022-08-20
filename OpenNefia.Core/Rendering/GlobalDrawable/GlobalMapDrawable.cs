using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public abstract class BaseGlobalDrawable : BaseDrawable, IGlobalDrawable
    {
        public bool IsFinished { get; protected set; }
        public override float UIScale => _uiScale;
        private float _uiScale = 1f;

        public BaseGlobalDrawable(float uiScale)
        {
            _uiScale = uiScale;
        }

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
