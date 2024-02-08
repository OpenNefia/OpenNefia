using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Content.ConfigMenu.UICell;

namespace OpenNefia.VisualAI.UserInterface
{
    /// <summary>
    /// A base UI for changing a variable value with the left/right keys in a list,
    /// similar to the config menu.
    /// </summary>
    /// <seealso cref="BaseConfigMenuUICell"/>
    public abstract class BaseDynamicVariableListCell : UiListCell<IDynamicVariableItem>
    {
        [Child] public UiText ValueTextElem { get; } = new UiText(UiFonts.ListText);
        [Child] protected AssetDrawable AssetArrowLeft;
        [Child] protected AssetDrawable AssetArrowRight;

        protected virtual bool ShowArrows => true;

        protected Color ColorArrowDisabled = Color.White.WithAlphaB(100);

        public BaseDynamicVariableListCell(IDynamicVariableItem data) : base(data)
        {
            AssetArrowLeft = new AssetDrawable(Protos.Asset.ArrowLeft);
            AssetArrowRight = new AssetDrawable(Protos.Asset.ArrowRight);
        }

        public virtual object? CurrentValue
        {
            get => Data.Variable.Value;
            set => Data.Variable.Value = value;
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

        public virtual void Change(int delta)
        {
        }

        public virtual void Activate()
        {
        }

        public bool Enabled { get; set; } = true;
        public abstract string ValueText { get; }

        public virtual void RefreshVariableValueDisplay()
        {
            UiText.Text = Data.Name;
            UiText.Color = Enabled ? UiColors.TextBlack : UiColors.TextDisabled;
            ValueTextElem.Text = ValueText;

            var (leftArrowEnabled, rightArrowEnabled) = CanChange();
            AssetArrowLeft.Color = leftArrowEnabled ? Color.White : ColorArrowDisabled;
            AssetArrowRight.Color = rightArrowEnabled ? Color.White : ColorArrowDisabled;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            ValueTextElem.SetPreferredSize();
            AssetArrowLeft.SetPreferredSize();
            AssetArrowRight.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            ValueTextElem.SetPosition(X + 194, Y);
            AssetArrowLeft.SetPosition(X + 164, Y - 5);
            AssetArrowRight.SetPosition(X + 302, Y - 5);
        }

        public override void Draw()
        {
            base.Draw();
            ValueTextElem.Draw();

            if (ShowArrows)
            {
                AssetArrowLeft.Draw();
                AssetArrowRight.Draw();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            ValueTextElem.Update(dt);
            AssetArrowLeft.Update(dt);
            AssetArrowRight.Update(dt);
        }
    }
}
