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
        private readonly List<IInventoryBehavior> Behaviours = new List<IInventoryBehavior>
        {
            new ExamineInventoryBehavior(),
            new DropInventoryBehavior(),
            new EatInventoryBehavior(),
            // TODO: read behaviour
            new DrinkInventoryBehavior(),
            // TODO: zap behaviour
            // TODO: use behaviour
            // TODO: open behaviour
            // TODO: mix behaviour
            new ThrowInventoryBehavior(),
        };

        public InventoryGroupArgs(EntityUid entity, IInventoryBehavior selected)
        {
            InventoryContext context = default!;
            foreach (var behaviour in Behaviours)
            {
                if (behaviour.GetType() == selected.GetType())
                {
                    context = new InventoryContext(entity, selected);
                    SelectedArgs = context;
                }
                else
                {
                    context = new InventoryContext(entity, behaviour);
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

            iconAsset.OriginOffset = (-12, -32);
            return iconAsset;
        }

        protected override string GetText(InventoryContext args)
        {
            return args.Behavior.WindowTitle;
        }
    }
}
