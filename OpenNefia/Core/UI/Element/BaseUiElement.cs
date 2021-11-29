using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.UI.Element
{
    [ImplicitDataDefinitionForInheritors]
    public abstract class BaseUiElement : BaseDrawable, IUiDefaultSizeable, ILocalizable
    {
        public bool IsLocalized { get; private set; }

        public virtual void GetPreferredSize(out int width, out int height)
        {
            width = 64;
            height = 64;
        }

        /// <summary>
        /// Called after <see cref="UiStyled" /> fields are applied to this element.
        /// </summary>
        public virtual void Initialize()
        {
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