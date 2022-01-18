using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Equipment
{
    public static class EquipmentHelpers
    {
        public static int GetTotalEquipmentWeight(EntityUid equipTarget, IEntityManager entityManager, IEquipSlotsSystem equipSlotsSystem)
        {
            if (!equipSlotsSystem.TryGetEquipSlots(equipTarget, out var equipSlots))
                return 0;

            var totalWeight = 0;

            foreach (var equipSlot in equipSlots)
            {
                if (!equipSlotsSystem.TryGetContainerForEquipSlot(equipTarget, equipSlot, out var containerSlot))
                    continue;

                if (!entityManager.IsAlive(containerSlot.ContainedEntity))
                    continue;

                var equipment = containerSlot.ContainedEntity.Value;

                if (!entityManager.TryGetComponent(equipment, out WeightComponent weight))
                    continue;

                totalWeight += weight.Weight;
            }

            return totalWeight;
        }

        public static string DisplayArmorClass(int weight)
        {
            if (weight >= EquipmentConstants.ArmorClassHeavyWeight)
            {
                return Loc.GetString("Elona.Equipment.ArmorClass.Heavy");
            }
            else if (weight >= EquipmentConstants.ArmorClassMediumWeight)
            {
                return Loc.GetString("Elona.Equipment.ArmorClass.Medium");
            }
            else
            {
                return Loc.GetString("Elona.Equipment.ArmorClass.Light");
            }
        }

        public static AssetDrawable MakeEquipSlotIcon(EquipSlotIcon icon)
        {
            if (!Enum.IsDefined(typeof(EquipSlotIcon), icon))
            {
                Logger.WarningS("inv", $"Missing equip slot icon {icon}");
                icon = EquipSlotIcon.Head;
            }

            return new AssetDrawable(Protos.Asset.EquipSlotIcons, regionId: ((int)icon).ToString());
        }
    }
}
