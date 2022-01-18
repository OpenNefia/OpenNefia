using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using static OpenNefia.Content.Prototypes.Protos;
using ConfigMenuItemProtoId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.ConfigMenu.ConfigMenuItemPrototype>;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    /// <summary>
    /// Vanilla UI view for the config menu item models. Wraps <see cref="IConfigMenuNode"/>
    /// with UI-specific properties.
    /// </summary>
    /// <remarks>
    /// NOTE: All inheritors must have a one-argument constructor that accepts 
    /// a <see cref="IConfigMenuNode"/>.
    /// </remarks>
    public abstract class BaseConfigMenuUICell : UiListCell<UINone>
    {
        public IConfigMenuNode MenuNode { get; }
        public UiText ValueText { get; } = new UiText(UiFonts.ListText);

        protected BaseConfigMenuUICell(IConfigMenuNode menuNode) : base(new(), "", null)
        {
            MenuNode = menuNode;
        }

        public virtual void RefreshConfigValueDisplay()
        {
        }

        public virtual void Initialize()
        {
        }

        public virtual (bool decArrow, bool incArrow) CanChange()
        {
            return (false, false);
        }

        public virtual bool CanActivate()
        {
            return false;
        }

        public virtual void HandleChanged(int delta)
        {
        }

        public virtual void HandleActivated()
        {
        }
    }

    /// <summary>
    /// Vanilla UI view for the config menu item models. Wraps <see cref="IConfigMenuNode"/>
    /// with UI-specific properties.
    /// </summary>
    /// <remarks>
    /// NOTE: All inheritors must have a one-argument constructor that accepts 
    /// a <see cref="IConfigMenuNode"/>.
    /// </remarks>
    public abstract class BaseConfigMenuUICell<TMenuNode> : BaseConfigMenuUICell
        where TMenuNode: IConfigMenuNode
    {
        protected readonly IConfigurationManager ConfigManager;

        protected ConfigMenuItemProtoId ProtoId { get; }

        protected AssetDrawable AssetArrowLeft;
        protected AssetDrawable AssetArrowRight;

        protected virtual bool ShowArrows => true;

        protected Color ColorArrowDisabled = Color.White.WithAlpha(100);

        public BaseConfigMenuUICell(ConfigMenuItemProtoId protoId, TMenuNode menuNode) 
            : base(menuNode)
        {
            ProtoId = protoId;
            ConfigManager = IoCManager.Resolve<IConfigurationManager>();

            AssetArrowLeft = new AssetDrawable(Asset.ArrowLeft);
            AssetArrowRight = new AssetDrawable(Asset.ArrowRight);
        }

        public new TMenuNode MenuNode => (TMenuNode)base.MenuNode;

        public bool Enabled { get; set; } = true;

        public override void RefreshConfigValueDisplay()
        {
            UiText.Text = Loc.GetPrototypeString(ProtoId, "Name");
            UiText.Color = Enabled ? UiColors.TextBlack : UiColors.TextDisabled;

            var (leftArrowEnabled, rightArrowEnabled) = CanChange();
            AssetArrowLeft.Color = leftArrowEnabled ? Color.White : ColorArrowDisabled;  
            AssetArrowRight.Color = rightArrowEnabled ? Color.White : ColorArrowDisabled;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            ValueText.SetPreferredSize();
            AssetArrowLeft.SetPreferredSize();
            AssetArrowRight.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            ValueText.SetPosition(X + 194, Y + 1);
            AssetArrowLeft.SetPosition(X + 164, Y - 5);
            AssetArrowRight.SetPosition(X + 302, Y - 5);
        }

        public override void Draw()
        {
            base.Draw();
            ValueText.Draw();

            if (ShowArrows)
            {
                AssetArrowLeft.Draw();
                AssetArrowRight.Draw();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            ValueText.Update(dt);
            AssetArrowLeft.Update(dt);
            AssetArrowRight.Update(dt);
        }
    }

    public abstract class BaseConfigMenuCVarUICell<TMenuNode, TCVar> : BaseConfigMenuUICell<TMenuNode>
        where TMenuNode : IConfigCVarMenuNode<TCVar>
        where TCVar: notnull
    {
        protected BaseConfigMenuCVarUICell(ConfigMenuItemProtoId protoId, TMenuNode data) : base(protoId, data)
        {
        }

        public TCVar CurrentValue
        {
            get => ConfigManager.GetCVar(MenuNode.CVar);
            set => ConfigManager.SetCVar(MenuNode.CVar, value);
        }
    }
}
