using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Inventory
{
    public class InventoryGroupArgs : UiGroupArgs<InventoryLayer, InventoryContext>
    {
        public InventoryGroupArgs(EntityUid entity, IList<IInventoryBehavior> behaviors, int selectedIndex = 0)
        {
            if (behaviors.Count == 0)
                throw new InvalidOperationException($"No inventory behaviors were provided to {nameof(InventoryGroupArgs)}!");

            selectedIndex = int.Clamp(selectedIndex, 0, behaviors.Count - 1);

            InventoryContext context = default!;
            for (var i = 0; i < behaviors.Count; i++)
            {
                var behavior = behaviors[i];
                context = new InventoryContext(entity, behavior);
                if (i == selectedIndex)
                {
                    SelectedArgs = context;
                }
                Layers[context] = new InventoryLayer();
            }
        }
    }

    public class InventoryUiGroup : UiGroup<InventoryLayer, InventoryGroupArgs, InventoryContext, InventoryLayer.Result>
    {
        protected override AssetDrawable? GetIcon(InventoryContext args)
        {
            var icon = args.Behavior.MakeIcon()!;
            if (icon is not AssetDrawable iconAsset)
                return null;

            return iconAsset;
        }

        protected override string GetTabName(InventoryContext args)
        {
            return args.Behavior.WindowTitle;
        }
    }
}
