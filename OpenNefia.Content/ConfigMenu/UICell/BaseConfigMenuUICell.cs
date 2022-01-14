using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI;
using ConfigMenuItemProtoId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.ConfigMenu.ConfigMenuItemPrototype>;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public abstract class BaseConfigMenuUICell : UiListCell<UINone>
    {
        public IConfigMenuNode MenuNode { get; }

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

        public virtual void HandleChanged(int delta)
        {
        }

        public virtual void HandleActivated()
        {
        }
    }

    /// <summary>
    /// Vanilla UI view for the config menu item models.
    /// </summary>
    public abstract class BaseConfigMenuUICell<TMenuNode> : BaseConfigMenuUICell
        where TMenuNode: IConfigMenuNode
    {
        protected readonly IConfigurationManager ConfigManager;

        protected ConfigMenuItemProtoId ProtoId { get; }
        protected IUiText ValueText = new UiText(UiFonts.ListText);

        public BaseConfigMenuUICell(ConfigMenuItemProtoId protoId, TMenuNode menuNode) 
            : base(menuNode)
        {
            ProtoId = protoId;
            ConfigManager = IoCManager.Resolve<IConfigurationManager>();
        }

        public new TMenuNode MenuNode => (TMenuNode)base.MenuNode;

        public bool Enabled { get; set; } = true;

        public override void RefreshConfigValueDisplay()
        {
            UiText.Text = Loc.GetPrototypeString(ProtoId, "Name");
            UiText.Color = Enabled ? UiColors.TextBlack : UiColors.TextDisabled;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            ValueText.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            ValueText.SetPosition(X + 194, Y + 1);
        }

        public override void Draw()
        {
            base.Draw();
            ValueText.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            ValueText.Draw();
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
