using OpenNefia.Content.Items;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.DisplayName
{
    public sealed partial class ItemNameSystem
    {
        private void Initialize_ItemEvents()
        {
            SubscribeComponent<LostPropertyComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_LostProperty);
        }

        private void LocalizeExtra_LostProperty(EntityUid uid, LostPropertyComponent component, ref LocalizeItemNameExtraEvent args)
        {
            // >>>>>>>> shade2/item_func.hsp:636 	if en@{ ...
            args.OutFullName.Append(Loc.GetString("Elona.Item.ItemName.LostProperty"));
            // <<<<<<<< shade2/item_func.hsp:638 		} ..
        }
    }
}
