using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.UI.Element
{
    public abstract class BaseUiElement : BaseDrawable, IUiDefaultSizeable, ILocalizable
    {
        public bool IsLocalized { get; protected set; }

        public virtual void GetPreferredSize(out Vector2i size)
        {
            size = new Vector2i(64, 64);
        }

        public void SetPreferredSize()
        {
            this.GetPreferredSize(out var size);
            this.SetSize(size.X, size.Y);
        }

        public virtual void Localize(LocaleKey key)
        {
            IoCManager.Resolve<ILocalizationManager>().DoLocalize(this, key);
            IsLocalized = true;
        }
    }
}