using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;

namespace OpenNefia.Core.UI.Element
{
    public abstract class BaseUiElement : BaseDrawable, IUiDefaultSizeable, ILocalizable
    {
        public bool IsLocalized { get; private set; }

        public virtual void GetPreferredSize(out int width, out int height)
        {
            width = 64;
            height = 64;
        }

        public void SetPreferredSize()
        {
            this.GetPreferredSize(out int width, out int height);
            this.SetSize(width, height);
        }

        public virtual void Localize(LocaleKey key)
        {
            IoCManager.Resolve<ILocalizationManager>().DoLocalize(this, key);
            IsLocalized = true;
        }
    }
}