using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Equipment;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Log;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Identify;

namespace OpenNefia.Content.Inventory
{
    public static class InventoryHelpers
    {
        /// <summary>
        /// Returns the color for displaying this item's name in the inventory/equipment screens.
        /// </summary>
        /// <param name="uid">The item's entity UID.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <returns></returns>
        public static Color GetItemTextColor(EntityUid uid, IEntityManager entityManager)
        {
            var pickable = entityManager.EnsureComponent<PickableComponent>(uid);

            if (pickable.IsNoDrop)
            {
                return UiColors.InventoryItemNoDrop;
            }

            var identified = true;
            if (entityManager.TryGetComponent(uid, out IdentifyComponent identify))
            {
                identified = identify.IdentifyState == IdentifyState.Full;
            }

            if (identified && entityManager.TryGetComponent(uid, out CurseStateComponent curse))
            {
                switch (curse.CurseState)
                {
                    case CurseState.Doomed:
                        return UiColors.InventoryItemDoomed;
                    case CurseState.Cursed:
                        return UiColors.InventoryItemCursed;
                    case CurseState.Normal:
                        return UiColors.InventoryItemNormal;
                    case CurseState.Blessed:
                        return UiColors.InventoryItemBlessed;
                }
            }

            return UiColors.TextBlack;
        }

        public static AssetDrawable MakeIcon(InventoryIcon icon)
        {
            if (!Enum.IsDefined(typeof(InventoryIcon), icon))
            {
                Logger.WarningS("inv", $"Missing inventory icon {icon}");
                icon = InventoryIcon.Drink;
            }

            return new AssetDrawable(Protos.Asset.InventoryIcons, regionId: ((int)icon).ToString());
        }
    }
}
