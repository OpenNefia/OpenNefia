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
using OpenNefia.Content.Identify;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.EquipSlots.EquipSlotPrototype>;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.Inventory
{
    public class IdentifyInventoryBehavior : BaseInventoryBehavior
    {
        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 13));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Identify.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.PickUp);

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new EntityEquipmentSource(context.Target);
            yield return new EntityInvSource(context.Target);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Identify.QueryText");
        }

        public override bool IsAccepted(InventoryContext context, EntityUid item)
        {
            return EntityManager.TryGetComponent<IdentifyComponent>(item, out var identify) 
                   && identify.IdentifyState != IdentifyState.Full;
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            return new InventoryResult.Finished(TurnResult.Succeeded);
        }
    }
}
