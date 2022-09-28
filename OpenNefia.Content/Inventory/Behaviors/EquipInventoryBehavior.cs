using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.EquipSlots.EquipSlotPrototype>;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.Inventory
{
    public class EquipInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IEquipmentSystem _equipmentSystem = default!;
        
        /// <summary>
        /// Equipment slot type to filter the items by.
        /// </summary>
        public EquipSlotPrototypeId EquipSlotId { get; }

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 6));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Equip.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.Equip);

        public EquipInventoryBehavior(EquipSlotPrototypeId equipSlotId)
        {
            EquipSlotId = equipSlotId;
        }

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new EntityInventorySource(context.Target);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Equip.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return _equipmentSystem.CanEquipItemInSlot(item, EquipSlotId);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            // TODO fairy trait
            // That will probably be a hook on IsEquippingAttemptEvent.
            
            return new InventoryResult.Finished(TurnResult.Succeeded);
        }
    }
}
