using OpenNefia.Content.Activity;
using OpenNefia.Content.Food;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Cargo
{
    public sealed class TravelersFoodSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IFoodSystem _foods = default!;

        public override void Initialize()
        {
            SubscribeComponent<InventoryComponent, OnTravelInWorldMapEvent>(ProcEatTravelersFood);
        }

        private void ProcEatTravelersFood(EntityUid uid, InventoryComponent component, OnTravelInWorldMapEvent args)
        {
            if (!TryComp<HungerComponent>(uid, out var hunger))
                return;

            if (hunger.Nutrition <= HungerLevels.Normal)
            {
                var travelersFood = _inv.EntityQueryInInventory<TravelersFoodComponent>(uid, inv: component)
                    .FirstOrDefault();

                if (travelersFood != null)
                {
                    _mes.Display(Loc.GetString("Elona.Activity.Eating.Finish", ("actor", uid), ("food", travelersFood.Owner)), entity: uid);
                    _foods.EatFood(uid, travelersFood.Owner);
                }
            }
        }
    }
}