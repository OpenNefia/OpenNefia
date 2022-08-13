using OpenNefia.Content.Activity;
using OpenNefia.Content.Food;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Cargo
{
    public sealed class TravelersFoodSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IFoodSystem _food = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<InventoryComponent, OnTravelInWorldMapEvent>(ProcEatTravelersFood);
        }

        private void ProcEatTravelersFood(EntityUid uid, InventoryComponent inv, ref OnTravelInWorldMapEvent args)
        {
            if (!_gameSession.IsPlayer(uid) || !TryComp<HungerComponent>(uid, out var hunger))
                return;

            if (hunger.Nutrition <= HungerLevels.Normal)
            {
                var travelersFood = _inv.EntityQueryInInventory<TravelersFoodComponent>(uid, inv: inv)
                    .FirstOrDefault();

                if (travelersFood != null)
                {
                    _mes.Display(Loc.GetString("Elona.Activity.Eating.Finish", ("actor", uid), ("food", travelersFood.Owner)), entity: uid);
                    _food.EatFood(uid, travelersFood.Owner);
                }
            }
        }
    }
}