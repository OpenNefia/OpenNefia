using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ConfigMenu
{
    public interface IConfigItemCell
    {
    }

    public class ConfigItemBooleanCell : UiListCell<bool>
    {
        public ConfigItemBooleanCell(bool data, UiListChoiceKey? key = null) : base(data, "", key)
        {
            RefreshConfigValueDisplay();
        }

        public bool Enabled { get; set; } = true;

        public (bool decArrow, bool incArrow) CanChange()
        {
            if (Data == true)
                return (false, true);
            else
                return (true, false);

        }

        public void OnActivate()
        {
        }

        public void OnChange(int delta)
        {
            if (delta > 0)
                Data = true;
            else
                Data = false;
        }

        protected void RefreshConfigValueDisplay()
        {
            var key = Data == true ? "Yes" : "No";
            var keyRoot = "Elona.Config.Common.YesNo.Default";

            UiText.Text = Loc.GetString($"{keyRoot}.{key}");
            UiText.Color = Enabled ? UiColors.TextBlack : UiColors.TextDisabled;
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}
