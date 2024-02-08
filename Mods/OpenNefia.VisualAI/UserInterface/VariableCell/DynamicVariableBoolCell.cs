using OpenNefia.Content.ConfigMenu.UICell;
using OpenNefia.Content.ConfigMenu;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.VisualAI.Engine;
using OpenNefia.Core;
using System.Runtime.InteropServices;

namespace OpenNefia.VisualAI.UserInterface
{
    public class DynamicVariableBoolCell : BaseDynamicVariableListCell
    {
        public DynamicVariableBoolCell(IDynamicVariableItem data) : base(data)
        {
        }

        public LocaleKey KeyRoot { get; set; } = "Elona.Config.Common.YesNo.Default";

        public bool InnerValue => (bool)base.CurrentValue!;

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (InnerValue == true, InnerValue == false);
        }

        public override void Change(int delta)
        {
            if (delta > 0)
                base.CurrentValue = true;
            else
                base.CurrentValue = false;
        }

        public override string ValueText
        {
            get
            {
                var key = InnerValue == true ? "Yes" : "No";
                return Loc.GetString($"{KeyRoot}.{key}");
            }
        }
    }
}
