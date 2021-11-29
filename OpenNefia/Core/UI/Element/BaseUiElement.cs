using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.UI.Element
{
    [ImplicitDataDefinitionForInheritors]
    public abstract class BaseUiElement : BaseDrawable, IUiThemeable, IUiDefaultSizeable, ILocalizable
    {
        public bool IsLocalized { get; protected set; }

        public virtual void GetPreferredSize(out Vector2i size)
        {
            size = new Vector2i(64, 64);
        }

        /// <summary>
        /// Called after <see cref="UiStyled" /> fields are applied to this element.
        /// </summary>
        public virtual void ApplyTheme()
        {
        }

        public void SetPreferredSize()
        {
            this.GetPreferredSize(out var size);
            this.SetSize(size);
        }

        public virtual void Localize(LocaleKey key)
        {
            IoCManager.Resolve<ILocalizationManager>().DoLocalize(this, key);
            IsLocalized = true;
        }
    }
}